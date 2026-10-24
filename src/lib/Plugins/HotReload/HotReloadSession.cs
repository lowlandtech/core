namespace LowlandTech.Core.Plugins.HotReload;

/// <summary>
/// Immutable snapshot of the progress for a single hot-reload session.
/// SPEC2050 US05.
/// </summary>
public sealed record HotReloadSession
{
    public required string PluginId { get; init; }
    public string? FromVersion { get; init; }
    public string? ToVersion { get; init; }
    public HotReloadPhase Phase { get; init; } = HotReloadPhase.Idle;
    public string? StagingPath { get; init; }
    public string? OldAlcName { get; init; }
    public string? NewAlcName { get; init; }

    /// <summary>
    /// Whether the old version's assemblies are preserved on disk for rollback.
    /// SPEC2050 US05 SC08.
    /// </summary>
    public bool PreviousVersionPreserved { get; init; }

    public string? Error { get; init; }
}
