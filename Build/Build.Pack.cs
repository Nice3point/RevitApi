using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    Target Pack => _ => _
        .TriggeredBy(Cleaning)
        .Executes(() =>
        {
            var configurations = GetConfigurations(BuildConfiguration);
            configurations.ForEach(configuration =>
            {
                foreach (var library in ContentDirectory.GlobFiles("*.dll"))
                {
                    DotNetPack(settings => settings
                        .SetConfiguration(configuration)
                        .SetVersion(PackVersion)
                        .SetPackageId($"Nice3point.Revit.Api.{library.NameWithoutExtension}")
                        .SetProperty("PackVersion", PackVersion)
                        .SetProperty("LibraryName", library.NameWithoutExtension)
                        .SetProperty("RevitFramework", RevitFramework[PackVersion[..4]])
                        .SetOutputDirectory(ArtifactsDirectory)
                        .SetVerbosity(DotNetVerbosity.Minimal));
                }
            });
        });
}