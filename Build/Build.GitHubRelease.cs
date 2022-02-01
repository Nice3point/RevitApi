using Nuke.Common.Git;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitVersion;
using Octokit;
using Serilog;

partial class Build
{
    [GitVersion(NoFetch = true)] readonly GitVersion GitVersion;
    [Parameter] string GitHubToken { get; set; }

    Target PublishGitHubRelease => _ => _
        .TriggeredBy(Pack)
        .Requires(() => GitHubToken)
        .Requires(() => GitRepository)
        .Requires(() => GitVersion)
        .OnlyWhenStatic(() => GitRepository.IsOnMainOrMasterBranch())
        .OnlyWhenStatic(() => IsServerBuild)
        .Executes(() =>
        {
            GitHubTasks.GitHubClient = new GitHubClient(new ProductHeaderValue(Solution.Name))
            {
                Credentials = new Credentials(GitHubToken)
            };

            var gitHubName = GitRepository.GetGitHubName();
            var gitHubOwner = GitRepository.GetGitHubOwner();
            var artifacts = Directory.GetFiles(ArtifactsDirectory, "*");
            var isPreRelease = PackVersion.Contains("-beta");

            CheckTags(gitHubOwner, gitHubName, PackVersion);
            Log.Information("Detected Tag: {Version}", PackVersion);

            var newRelease = new NewRelease(PackVersion)
            {
                Name = PackVersion,
                Draft = true,
                Prerelease = isPreRelease,
                TargetCommitish = GitVersion.Sha
            };

            var draft = CreatedDraft(gitHubOwner, gitHubName, newRelease);
            UploadArtifacts(draft, artifacts);
            ReleaseDraft(gitHubOwner, gitHubName, draft);
        });

    static void CheckTags(string gitHubOwner, string gitHubName, string version)
    {
        var gitHubTags = GitHubTasks.GitHubClient.Repository
            .GetAllTags(gitHubOwner, gitHubName)
            .Result;

        if (gitHubTags.Select(tag => tag.Name).Contains(version)) throw new ArgumentException($"The repository already contains a Release with the tag: {version}");
    }

    static void UploadArtifacts(Release createdRelease, IEnumerable<string> artifacts)
    {
        foreach (var file in artifacts)
        {
            var releaseAssetUpload = new ReleaseAssetUpload
            {
                ContentType = "application/x-binary",
                FileName = Path.GetFileName(file),
                RawData = File.OpenRead(file)
            };
            var _ = GitHubTasks.GitHubClient.Repository.Release.UploadAsset(createdRelease, releaseAssetUpload).Result;
            Log.Information("Added artifact: {Path}", file);
        }
    }

    static Release CreatedDraft(string gitHubOwner, string gitHubName, NewRelease newRelease) =>
        GitHubTasks.GitHubClient.Repository.Release
            .Create(gitHubOwner, gitHubName, newRelease)
            .Result;

    static void ReleaseDraft(string gitHubOwner, string gitHubName, Release draft)
    {
        var _ = GitHubTasks.GitHubClient.Repository.Release
            .Edit(gitHubOwner, gitHubName, draft.Id, new ReleaseUpdate {Draft = false})
            .Result;
    }
}