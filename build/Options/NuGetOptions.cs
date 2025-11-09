using System.ComponentModel.DataAnnotations;
using ModularPipelines.Attributes;

namespace Build.Options;

[Serializable]
public sealed record NuGetOptions
{
    [Required] [SecretValue] public string ApiKey { get; init; } = null!;
    [Required] public string Source { get; init; } = null!;
}