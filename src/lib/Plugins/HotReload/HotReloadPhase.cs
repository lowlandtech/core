namespace LowlandTech.Core.Plugins.HotReload;

/// <summary>
/// Lifecycle phase of a single hot-reload session for a plugin.
/// SPEC2050 US05.
/// </summary>
public enum HotReloadPhase
{
    /// <summary>No update in progress.</summary>
    Idle,

    /// <summary>A newer version has been detected (via poll or webhook).</summary>
    Detected,

    /// <summary>The new version is being downloaded and extracted to staging.</summary>
    Staging,

    /// <summary>The new version has been fully staged and is ready to swap.</summary>
    Staged,

    /// <summary>The old ALC is being suspended.</summary>
    Suspending,

    /// <summary>The new ALC is being loaded.</summary>
    Swapping,

    /// <summary>The new ALC is being resumed.</summary>
    Resuming,

    /// <summary>The update was applied successfully.</summary>
    Applied,

    /// <summary>The swap failed and rollback is in progress.</summary>
    RollingBack,

    /// <summary>Rollback succeeded; the previous ALC is active again.</summary>
    RolledBack,

    /// <summary>Rollback failed (no previous version available) — manual intervention required.</summary>
    Error,
}
