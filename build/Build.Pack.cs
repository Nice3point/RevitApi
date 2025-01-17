using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

sealed partial class Build
{
    Target Pack => _ => _
        .DependsOn(Clean)
        .OnlyWhenStatic(() => !string.IsNullOrEmpty(AssemblyName) || !string.IsNullOrEmpty(ReleaseVersion))
        .Executes(() =>
        {
            var readme = CreateNugetReadme();
            try
            {
                if (string.IsNullOrEmpty(ReleaseVersion))
                {
                    foreach (var contentDirectory in RootContentDirectory.GlobDirectories("*"))
                    {
                        PackFiles(contentDirectory);
                    }
                }
                else
                {
                    PackFiles(RootContentDirectory / ReleaseVersion);
                }
            }
            finally
            {
                RestoreReadme(readme);
            }
        });

    void PackFiles(AbsolutePath contentDirectory)
    {
        foreach (var library in contentDirectory.GlobFiles("*.dll"))
        {
            if (!string.IsNullOrEmpty(AssemblyName) && library.NameWithoutExtension != AssemblyName) continue;

            var version = library.Parent!.Name;
            DotNetPack(settings => settings
                .SetConfiguration("Release")
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

        const string startSymbol = "<p";
        const string endSymbol = "</p>\r\n\r\n";

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