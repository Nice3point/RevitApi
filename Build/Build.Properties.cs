partial class Build
{
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
    };

    const string PackVersion = "2023.0.1";
    const string LibName = "RevitAddInUtility"; // Make it empty to build all libraries
    const string BuildConfiguration = "Release";
    const string ArtifactsFolder = "output";
}