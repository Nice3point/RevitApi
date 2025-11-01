using System.ComponentModel.DataAnnotations;

namespace Build.Options;

public sealed class ReleaseOptions
{
    [Required] public string Changelog { get; init; } = null!;
}