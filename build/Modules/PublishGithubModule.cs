using Build.Options;
using EnumerableAsyncProcessor.Extensions;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.Git.Extensions;
using ModularPipelines.GitHub.Attributes;
using ModularPipelines.GitHub.Extensions;
using ModularPipelines.Modules;
using Octokit;
using Shouldly;

namespace Build.Modules;

[SkipIfNoGitHubToken]
[DependsOn<PackProjectsModule>]
public sealed class PublishGithubModule(IOptions<PackOptions> packOptions, IOptions<ReleaseOptions> releaseOptions) : Module<ReleaseAsset[]?>
{
    protected override async Task<ReleaseAsset[]?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
    {
        var outputFolder = context.Git().RootDirectory.GetFolder(packOptions.Value.OutputDirectory);
        var targetFiles = outputFolder.ListFiles().ToArray();
        targetFiles.Length.ShouldBePositive("No artifacts were found to create the Release");

        var repositoryInfo = context.GitHub().RepositoryInfo;
        var newRelease = new NewRelease(releaseOptions.Value.Version)
        {
            Name = releaseOptions.Value.Version,
            Body = releaseOptions.Value.Changelog,
            TargetCommitish = context.Git().Information.LastCommitSha,
            Prerelease = releaseOptions.Value.Version.Contains("preview")
        };

        var release = await context.GitHub().Client.Repository.Release.Create(repositoryInfo.Owner, repositoryInfo.RepositoryName, newRelease);

        return await targetFiles
            .SelectAsync(async file =>
                {
                    var asset = new ReleaseAssetUpload
                    {
                        ContentType = "application/x-binary",
                        FileName = file.Name,
                        RawData = file.GetStream()
                    };

                    return await context.GitHub().Client.Repository.Release.UploadAsset(release, asset, cancellationToken);
                },
                cancellationToken)
            .ProcessInParallel();
    }
}