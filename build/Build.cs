using JetBrains.Annotations;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;

[PublicAPI]
sealed partial class Build : NukeBuild
{
    Dictionary<string, string> RevitFramework;
    
    [GitRepository] readonly GitRepository GitRepository;
    [Solution(GenerateProjects = true)] readonly Solution Solution;

    public static int Main() => Execute<Build>();
}