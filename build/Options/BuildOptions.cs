using System.ComponentModel.DataAnnotations;

namespace Build.Options;

[Serializable]
public sealed record BuildOptions
{
    [Required] public string Version { get; init; } = null!;
}