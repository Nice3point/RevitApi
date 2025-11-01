using Build.Modules;
using Build.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularPipelines.Extensions;
using ModularPipelines.Host;

await PipelineHostBuilder.Create()
    .ConfigureAppConfiguration((context, builder) =>
    {
        builder.AddJsonFile("appsettings.json")
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, collection) =>
    {
        if (args.Contains("delete-nuget"))
        {
            collection.AddOptions<ReleaseOptions>().Bind(context.Configuration.GetSection("Release")).ValidateDataAnnotations();
            collection.AddModule<DeleteNugetModule>();
            return;
        }

        if (args.Length == 0 || args.Contains("pack"))
        {
            collection.AddOptions<PackOptions>().Bind(context.Configuration.GetSection("Pack")).ValidateDataAnnotations();

            collection.AddModule<CleanProjectsModule>();
            collection.AddModule<CreatePackageReadmeModule>();
            collection.AddModule<PackProjectsModule>();
            collection.AddModule<RestoreReadmeModule>();
        }

        if (args.Contains("publish") && context.HostingEnvironment.IsProduction())
        {
            collection.AddOptions<ReleaseOptions>().Bind(context.Configuration.GetSection("Release")).ValidateDataAnnotations();
            collection.AddOptions<NuGetOptions>().Bind(context.Configuration.GetSection("NuGet")).ValidateDataAnnotations();

            collection.AddModule<PublishNugetModule>();
            collection.AddModule<PublishGithubModule>();
        }
    })
    .RunCategories(args)
    .ExecutePipelineAsync();