using ModularPipelines.Attributes;

namespace Build.Options;

public sealed class NuGetOptions
{
    [SecretValue] public string? ApiKey { get; init; }
    public string? Source { get; init; }
}