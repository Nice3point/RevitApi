using System.ComponentModel.DataAnnotations;

namespace Build.Options;

public sealed class BuildOptions
{
    [Required] public string Version { get; init; } = null!;
}