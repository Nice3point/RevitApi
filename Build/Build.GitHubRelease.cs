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
        .Executes(async () =>
        {
            if (string.IsNullOrEmpty(PackVersion))
            {
                Log.Information("Skipping GitHub release because no new versions was found");
                return;
            }

            GitHubTasks.GitHubClient = new GitHubClient(new ProductHeaderValue(Solution.Name))
            {
                Credentials = new Credentials(GitHubToken)
            };

            var gitHubName = GitRepository.GetGitHubName();
            var gitHubOwner = GitRepository.GetGitHubOwner();
            var artifacts = Directory.GetFiles(ArtifactsDirectory, "*");
            var isPreRelease = PackVersion.Contains("-beta") || PackVersion.Contains("-dev") || PackVersion.Contains("-preview");

            await CheckTagsAsync(gitHubOwner, gitHubName, PackVersion);
            Log.Information("Detected Tag: {Version}", PackVersion);

            var newRelease = new NewRelease(PackVersion)
            {
                Name = PackVersion,
                Draft = true,
                Prerelease = isPreRelease,
                TargetCommitish = GitVersion.Sha
            };

            var draft = await CreatedDraftAsync(gitHubOwner, gitHubName, newRelease);
            await UploadArtifactsAsync(draft, artifacts);
            await ReleaseDraftAsync(gitHubOwner, gitHubName, draft);
        });

    static async Task CheckTagsAsync(string gitHubOwner, string gitHubName, string version)
    {
        var gitHubTags = await GitHubTasks.GitHubClient.Repository.GetAllTags(gitHubOwner, gitHubName);
        if (gitHubTags.Select(tag => tag.Name).Contains(version)) throw new ArgumentException($"The repository already contains a Release with the tag: {version}");
    }

    static async Task UploadArtifactsAsync(Release createdRelease, IEnumerable<string> artifacts)
    {
        foreach (var file in artifacts)
        {
            var releaseAssetUpload = new ReleaseAssetUpload
            {
                ContentType = "application/x-binary",
                FileName = Path.GetFileName(file),
                RawData = File.OpenRead(file)
            };
            await GitHubTasks.GitHubClient.Repository.Release.UploadAsset(createdRelease, releaseAssetUpload);
            Log.Information("Added artifact: {Path}", file);
        }
    }

    static Task<Release> CreatedDraftAsync(string gitHubOwner, string gitHubName, NewRelease newRelease) =>
        GitHubTasks.GitHubClient.Repository.Release.Create(gitHubOwner, gitHubName, newRelease);

    static async Task ReleaseDraftAsync(string gitHubOwner, string gitHubName, Release draft) =>
        await GitHubTasks.GitHubClient.Repository.Release.Edit(gitHubOwner, gitHubName, draft.Id, new ReleaseUpdate {Draft = false});
}