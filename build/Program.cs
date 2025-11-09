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
            collection.AddOptions<BuildOptions>().Bind(context.Configuration.GetSection("Build")).ValidateDataAnnotations();
            collection.AddOptions<PackOptions>().Bind(context.Configuration.GetSection("Pack")).ValidateDataAnnotations();
            collection.AddOptions<NuGetOptions>().Bind(context.Configuration.GetSection("NuGet")).ValidateDataAnnotations();

            collection.AddModule<DeleteNugetModule>();
            return;
        }

        collection.AddOptions<PackOptions>().Bind(context.Configuration.GetSection("Pack")).ValidateDataAnnotations();

        collection.AddModule<CleanProjectsModule>();
        collection.AddModule<CreatePackageReadmeModule>();
        collection.AddModule<PackProjectsModule>();
        collection.AddModule<RestoreReadmeModule>();

        if (args.Contains("publish"))
        {
            if (!context.HostingEnvironment.IsProduction())
            {
                throw new InvalidOperationException("Publish can only be run in production");
            }

            collection.AddOptions<BuildOptions>().Bind(context.Configuration.GetSection("Build")).ValidateDataAnnotations();
            collection.AddOptions<ReleaseOptions>().Bind(context.Configuration.GetSection("Release")).ValidateDataAnnotations();
            collection.AddOptions<NuGetOptions>().Bind(context.Configuration.GetSection("NuGet")).ValidateDataAnnotations();

            collection.AddModule<PublishNugetModule>();
            collection.AddModule<PublishGithubModule>();
        }
    })
    .ExecutePipelineAsync();