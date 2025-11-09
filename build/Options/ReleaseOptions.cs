using System.ComponentModel.DataAnnotations;

namespace Build.Options;

[Serializable]
public sealed record ReleaseOptions
{
    [Required] public string Changelog { get; init; } = null!;
}