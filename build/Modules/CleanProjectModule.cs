using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.FileSystem;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Modules;
using RevitApi.Pipeline.Attributes;
using RevitApi.Pipeline.Options;
using Sourcy.DotNet;

namespace RevitApi.Pipeline.Modules;

[SkipIfHasGitHubToken]
[ModuleCategory("Publish")]
public sealed class CleanProjectModule(IOptions<PackOptions> packOptions) : Module
{
    protected override async Task<IDictionary<string, object>?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
    {
        context.Git().RootDirectory
            .GetFolders(TargetFoldersPredicate)
            .Where(IgnoredFoldersPredicate)
            .ToList()
            .ForEach(folder => folder.Delete());

        return await NothingAsync();
    }

    private bool TargetFoldersPredicate(Folder folder)
    {
        if (folder.Name == "bin")
        {
            return true;
        }

        if (folder.Name == "obj")
        {
            return true;
        }

        if (folder.Name == packOptions.Value.OutputDirectory)
        {
            return true;
        }

        return false;
    }
    
    private static bool IgnoredFoldersPredicate(Folder folder)
    {
        if (folder.Path.StartsWith(Projects.Build.Directory!.FullName))
        {
            return false;
        }
        
        return true;
    }
}