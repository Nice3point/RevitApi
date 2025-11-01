using System.ComponentModel.DataAnnotations;

namespace RevitApi.Pipeline.Options;

public sealed class ReleaseOptions
{
    [Required] public string Version { get; init; } = null!;
    [Required] public string Changelog { get; init; } = null!;
}