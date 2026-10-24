namespace LowlandTech.Core.Abstractions.Pluginize;

/// <summary>
/// Output from the pluginize pipeline.
/// SPEC2300 US30.
/// </summary>
public record PluginizeResult
{
    public int ExitCode { get; init; }
    public int Created { get; init; }
    public int Skipped { get; init; }
    public int Overwritten { get; init; }
    public int Renamed { get; init; }
    public int Pruned { get; init; }
    public IReadOnlyList<string> ConflictFiles { get; init; } = [];
    public IReadOnlyList<string> UserEditSkips { get; init; } = [];
    public string? ErrorMessage { get; init; }
}
