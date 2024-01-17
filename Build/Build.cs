using Nuke.Common.Git;
using Nuke.Common.ProjectModel;

partial class Build : NukeBuild
{
    string[] Configurations;
    Dictionary<string, string> RevitFramework;

    [Secret] [Parameter] string GitHubToken;
    [GitRepository] readonly GitRepository GitRepository;
    [Solution(GenerateProjects = true)] readonly Solution Solution;

    public static int Main() => Execute<Build>(x => x.Clean);
}