using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    Target Pack => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            ValidateRelease();

            foreach (var configuration in GlobBuildConfigurations())
            {
                if (string.IsNullOrEmpty(Version))
                {
                    foreach (var contentDirectory in RootContentDirectory.GlobDirectories("*"))
                    {
                        PackFiles(configuration, contentDirectory, contentDirectory.Name);
                    }
                }
                else
                {
                    PackFiles(configuration, ContentDirectory, Version);
                }
            }
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