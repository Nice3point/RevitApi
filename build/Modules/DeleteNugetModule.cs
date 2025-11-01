using Build.Options;
using EnumerableAsyncProcessor.Extensions;
using Microsoft.Extensions.Options;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Models;
using ModularPipelines.Modules;

namespace Build.Modules;

public sealed class DeleteNugetModule(
    IOptions<ReleaseOptions> buildOptions, 
    IOptions<PackOptions> packOptions, 
    IOptions<NuGetOptions> nuGetOptions)
    : Module<CommandResult[]?>
{
    protected override async Task<CommandResult[]?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
    {
        var targetFiles = context.Git().RootDirectory
            .GetFolder(packOptions.Value.ContentDirectory)
            .GetFiles(file => file.Extension == ".dll")
            .DistinctBy(file => file.Name);

        return await targetFiles
            .SelectAsync(async file => await context.DotNet().Nuget.Delete(new DotNetNugetDeleteOptions
                {
                    PackageNamePackageVersion = $"Nice3point.Revit.Api.{file.NameWithoutExtension} {buildOptions.Value.Version}",
                    ApiKey = nuGetOptions.Value.ApiKey,
                    Source = nuGetOptions.Value.Source,
                    NonInteractive = true
                }, cancellationToken),
                cancellationToken)
            .ProcessOneAtATime();
    }
}