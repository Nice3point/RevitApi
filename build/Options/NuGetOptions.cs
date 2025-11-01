using ModularPipelines.Attributes;

namespace RevitApi.Pipeline.Options;

public sealed class NuGetOptions
{
    [SecretValue] public string? ApiKey { get; init; }
    public string? Source { get; init; }
}