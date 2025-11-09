using System.ComponentModel.DataAnnotations;

namespace Build.Options;

[Serializable]
public sealed record PackOptions
{
    [Required] public string OutputDirectory { get; init; } = null!;
    [Required] public string ContentDirectory { get; init; } = null!;
    public string PinnedDllVersion { get; init; } = string.Empty;
    public string PinnedDllName { get; init; } = string.Empty;
}