using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularPipelines.Extensions;
using ModularPipelines.Host;
using RevitApi.Pipeline.Modules;
using RevitApi.Pipeline.Options;

if (args.Length == 0)
{
    throw new ArgumentException("No categories specified");
}

await PipelineHostBuilder.Create()
    .ConfigureAppConfiguration((context, builder) =>
    {
        builder.AddJsonFile("appsettings.json")
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, collection) =>
    {
        collection.AddOptions<NuGetOptions>().Bind(context.Configuration.GetSection("NuGet")).ValidateDataAnnotations();
        collection.AddOptions<PackOptions>().Bind(context.Configuration.GetSection("Pack")).ValidateDataAnnotations();
        collection.AddOptions<ReleaseOptions>().Bind(context.Configuration.GetSection("Release")).ValidateDataAnnotations();

        collection.AddModule<CleanProjectModule>();
        collection.AddModule<CreateNugetReadmeModule>();
        collection.AddModule<PackProjectsModule>();
        collection.AddModule<RestoreReadmeModule>();
        collection.AddModule<DeleteNugetModule>();

        if (context.HostingEnvironment.IsProduction())
        {
            collection.AddModule<PublishNugetModule>();
            collection.AddModule<PublishGithubModule>();
        }
    })
    .RunCategories(args)
    .ExecutePipelineAsync();