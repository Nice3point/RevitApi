sealed partial class Build
{
    [Parameter("Publish a new package with a specific DLL name for all versions")] string AssemblyName;
    [Parameter("Publish packages with a specific Release version")] static string ReleaseVersion;
    [Parameter("Release notes for the publication")] static string ReleaseNotes = "Build 26.3.0.37";

    readonly AbsolutePath ArtifactsDirectory = RootDirectory / "output";
    readonly AbsolutePath RootContentDirectory = RootDirectory / "RevitApi" / "Content";

    protected override void OnBuildInitialized()
    {
        ReleaseVersion ??= GitRepository.Tags.SingleOrDefault();
        
        RevitFramework = new()
        {
            { "2014", "net40" },
            { "2015", "net45" },
            { "2016", "net452" },
            { "2017", "net452" },
            { "2018", "net46" },
            { "2019", "net47" },
            { "2020", "net47" },
            { "2021", "net48" },
            { "2022", "net48" },
            { "2023", "net48" },
            { "2024", "net48" },
            { "2025", "net8.0" },
            { "2026", "net8.0" }
        };
    }
}