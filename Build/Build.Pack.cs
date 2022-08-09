using Nuke.Common.Tools.DotNet;
using Serilog;
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
                if (string.IsNullOrEmpty(PackVersion))
                {
                    foreach (var contentDirectory in RootContentDirectory.GlobDirectories("2*"))
                    {
                        PackFiles(configuration, contentDirectory, contentDirectory.Name);
                    }
                }
                else
                {
                    PackFiles(configuration, ContentDirectory, PackVersion);
                }
            });
        });

    void PackFiles(string configuration, AbsolutePath contentDirectory, string version)
    {
        foreach (var library in contentDirectory.GlobFiles("*.dll"))
        {
            if (!string.IsNullOrWhiteSpace(LibName) && !library.NameWithoutExtension.Equals(LibName)) continue;

            DotNetPack(settings => settings
                .SetConfiguration(configuration)
                .SetVersion(version)
                .SetPackageId($"Nice3point.Revit.Api.{library.NameWithoutExtension}")
                .SetProperty("PackVersion", version)
                .SetProperty("LibraryName", library.NameWithoutExtension)
                .SetProperty("RevitFramework", RevitFramework[version[..4]])
                .SetOutputDirectory(ArtifactsDirectory)
                .SetVerbosity(DotNetVerbosity.Minimal));
        }
    }
}