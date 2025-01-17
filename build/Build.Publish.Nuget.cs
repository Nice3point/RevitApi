using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

sealed partial class Build
{
    const string NugetSource = "https://api.nuget.org/v3/index.json";
    [Parameter] [Secret] string NugetApiKey = EnvironmentInfo.GetVariable("NUGET_API_KEY");

    Target PublishNuget => _ => _
        .DependsOn(Pack, PublishGitHub)
        .Requires(() => NugetApiKey)
        .Executes(() =>
        {
            foreach (var package in ArtifactsDirectory.GlobFiles("*.nupkg"))
            {
                DotNetNuGetPush(settings => settings
                    .SetTargetPath(package)
                    .SetApiKey(NugetApiKey)
                    .SetSource(NugetSource));
            }
        });

    Target DeleteNuGet => _ => _
        .Requires(() => NugetApiKey)
        .OnlyWhenStatic(() => IsLocalBuild)
        .Executes(() =>
        {
            foreach (var package in ArtifactsDirectory.GlobFiles("*.nupkg"))
            {
                DotNetNuGetDelete(settings => settings
                    .SetPackageId(package.Name[..package.Name.IndexOf(".20", StringComparison.Ordinal)])
                    .SetPackageVersion(ReleaseVersion)
                    .SetSource(NugetSource)
                    .SetApiKey(NugetApiKey)
                    .EnableNonInteractive());
            }
        });
}