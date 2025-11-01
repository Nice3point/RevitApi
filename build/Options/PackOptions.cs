using System.ComponentModel.DataAnnotations;

namespace Build.Options;

public sealed class PackOptions
{
    [Required] public string OutputDirectory { get; init; } = null!;
    [Required] public string ContentDirectory { get; init; } = null!;
    public string PinnedDllVersion { get; init; } = string.Empty;
    public string PinnedDllName { get; init; } = string.Empty;
}