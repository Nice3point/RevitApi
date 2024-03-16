sealed partial class Build
{
    const string LibName = ""; // Make it empty to build all libraries
    const string Version = "2025.0.0-preview.3.240313"; // Make it empty to build all versions

    readonly AbsolutePath ArtifactsDirectory = RootDirectory / "output";
    readonly AbsolutePath RootContentDirectory = RootDirectory / "RevitApi" / "Content";
    readonly AbsolutePath ContentDirectory = RootDirectory / "RevitApi" / "Content" / Version;

    protected override void OnBuildInitialized()
    {
        Configurations =
        [
            "Release*",
            "Installer*"
        ];

        RevitFramework = new()
        {
            {"2014", "net40"},
            {"2015", "net45"},
            {"2016", "net452"},
            {"2017", "net452"},
            {"2018", "net46"},
            {"2019", "net47"},
            {"2020", "net47"},
            {"2021", "net48"},
            {"2022", "net48"},
            {"2023", "net48"},
            {"2024", "net48"},
            {"2025", "net8.0"}
        };
    }
}