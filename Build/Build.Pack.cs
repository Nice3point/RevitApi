using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    Target Pack => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            ValidateRelease();

            var readme = CreateNugetReadme();
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

            RestoreReadme(readme);
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
                .SetVerbosity(DotNetVerbosity.minimal));
        }
    }

    string CreateNugetReadme()
    {
        var readmePath = Solution.Directory / "Readme.md";
        var readme = File.ReadAllText(readmePath);

        var startSymbol = "<p";
        var endSymbol = "</p>\r\n\r\n";
        var logoStartIndex = readme.IndexOf(startSymbol, StringComparison.Ordinal);
        var logoEndIndex = readme.IndexOf(endSymbol, StringComparison.Ordinal);

        var nugetReadme = readme.Remove(logoStartIndex, logoEndIndex - logoStartIndex + endSymbol.Length);
        File.WriteAllText(readmePath, nugetReadme);

        return readme;
    }

    void RestoreReadme(string readme)
    {
        var readmePath = Solution.Directory / "Readme.md";

        File.WriteAllText(readmePath, readme);
    }
}