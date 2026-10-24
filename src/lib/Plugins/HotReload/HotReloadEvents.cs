namespace LowlandTech.Core.Plugins.HotReload;

/// <summary>
/// Emitted when a newer version of a plugin is detected via polling or webhook push.
/// SPEC2050 US05 SC01 UAC03 / SC02 UAC02.
/// </summary>
public sealed record UpdateAvailable(string PluginId, string NewVersion);

/// <summary>
/// Emitted when a new version has been fully downloaded and extracted to the staging directory.
/// SPEC2050 US05 SC04 UAC04.
/// </summary>
public sealed record VersionStaged(string PluginId, string StagingPath);

/// <summary>
/// Emitted when the suspend/swap/resume handshake completes successfully.
/// SPEC2050 US05 SC05 UAC07.
/// </summary>
public sealed record UpdateApplied(string PluginId, string FromVersion, string ToVersion, string OldAlcName, string NewAlcName);

/// <summary>
/// Emitted when the swap fails and rollback was executed (or attempted).
/// SPEC2050 US05 SC07 UAC06 / SC09 UAC02.
/// </summary>
public sealed record UpdateFailed(string PluginId, string Version, string Reason);

/// <summary>
/// Emitted when a rollback completes successfully — the previous ALC is active again.
/// SPEC2050 US05 SC07.
/// </summary>
public sealed record RollbackCompleted(string PluginId, string RestoredVersion);

/// <summary>
/// Emitted when a downgrade request is rejected because <see cref="HotReloadConfig.AllowDowngrade"/> is <c>false</c>.
/// SPEC2050 US05 SC12 UAC03.
/// </summary>
public sealed record DowngradeRejected(string PluginId, string RequestedVersion, string InstalledVersion);
