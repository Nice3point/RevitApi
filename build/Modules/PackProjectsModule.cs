using Build.Options;
using EnumerableAsyncProcessor.Extensions;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.FileSystem;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Models;
using ModularPipelines.Modules;
using Sourcy.DotNet;
using File = ModularPipelines.FileSystem.File;

namespace Build.Modules;

[DependsOn<CleanProjectModule>]
[DependsOn<CreateNugetReadmeModule>]
[ModuleCategory("Publish")]
public sealed class PackProjectsModule(IOptions<PackOptions> packOptions) : Module<ICollection<CommandResult>>
{
    private readonly Dictionary<string, string> _revitFrameworks = new()
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
        { "2025", "net8.0-windows7.0" },
        { "2026", "net8.0-windows7.0" }
    };

    protected override async Task<ICollection<CommandResult>?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
    {
        var rootContentFolder = context.Git().RootDirectory.GetFolder(packOptions.Value.ContentDirectory);
        var outputFolder = context.Git().RootDirectory.GetFolder(packOptions.Value.OutputDirectory);
        var versionContentFolder = rootContentFolder.GetFolder(packOptions.Value.PinnedDllVersion);
        var targetFolders = string.IsNullOrEmpty(packOptions.Value.PinnedDllVersion)
            ? rootContentFolder.ListFolders()
            : [versionContentFolder];

        var packagedItems = new List<CommandResult>();
        foreach (var targetFolder in targetFolders)
        {
            var targetFiles = string.IsNullOrEmpty(packOptions.Value.PinnedDllName)
                ? targetFolder.GetFiles(file => file.Extension == ".dll")
                : targetFolder.GetFiles(file => file.Extension == ".dll" && file.NameWithoutExtension == packOptions.Value.PinnedDllName);

            var results = await PackAsync(context, targetFiles, outputFolder, cancellationToken);
            packagedItems.AddRange(results);
        }

        if (packagedItems.Count == 0) throw new Exception("No libraries were packed. Check build configuration.");
        return packagedItems;
    }

    private async Task<CommandResult[]> PackAsync(IPipelineContext context, IEnumerable<File> files, Folder output, CancellationToken cancellationToken)
    {
        return await files
            .SelectAsync(async file =>
            {
                var version = file.Folder!.Name;
                return await context.DotNet().Pack(new DotNetPackOptions
                {
                    ProjectSolution = Projects.Nice3point_Revit_Api.FullName,
                    Configuration = Configuration.Release,
                    Verbosity = Verbosity.Minimal,
                    Properties = new List<KeyValue>
                    {
                        ("Version", version),
                        ("PackageId", $"Nice3point.Revit.Api.{file.NameWithoutExtension}"),
                        ("LibraryName", file.NameWithoutExtension),
                        ("RevitFramework", _revitFrameworks[version[..4]])
                    },
                    OutputDirectory = output
                }, cancellationToken);
            }, cancellationToken)
            .ProcessOneAtATime();
    }
}