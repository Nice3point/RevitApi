partial class Build
{
    const string BuildConfiguration = "Release";
    const string ArtifactsFolder = "output";
    const string LibName = "UIFrameworkServices"; // Make it empty to build all libraries
    readonly string PackVersion = ""; // Make it empty to build all versions

    readonly Dictionary<string, string> RevitFramework = new()
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
        {"2024", "net48"}
    };
}