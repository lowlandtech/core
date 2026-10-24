namespace LowlandTech.Core.Plugins.HotReload;

/// <summary>
/// Orchestrates the ALC-swap hot-reload pipeline: detect → stage → suspend/swap/resume → rollback.
/// SPEC2050 US05.
/// </summary>
public interface IHotReloadService
{
    /// <summary>
    /// Returns the current hot-reload session state for the given plugin,
    /// or a new <see cref="HotReloadPhase.Idle"/> session if no session exists.
    /// </summary>
    HotReloadSession GetSession(string pluginId);

    /// <summary>
    /// Polls the feed for a newer version of <paramref name="pluginId"/>.
    /// Emits <see cref="HotReloadEvents.UpdateAvailable"/> if a newer version is found.
    /// Respects <see cref="HotReloadConfig.AllowPrerelease"/> and <see cref="HotReloadConfig.AllowDowngrade"/>.
    /// SPEC2050 US05 SC01 / SC03 / SC10 / SC12.
    /// </summary>
    /// <returns><c>true</c> if a newer version was detected.</returns>
    Task<bool> PollAsync(string pluginId, CancellationToken ct = default);

    /// <summary>
    /// Accepts a webhook push notification for a new version of <paramref name="pluginId"/>.
    /// Emits <see cref="HotReloadEvents.UpdateAvailable"/> immediately.
    /// Respects <see cref="HotReloadConfig.AllowPrerelease"/> and <see cref="HotReloadConfig.AllowDowngrade"/>.
    /// SPEC2050 US05 SC02 / SC11 / SC12.
    /// </summary>
    /// <returns><c>true</c> if the notification was accepted (not filtered or rejected).</returns>
    Task<bool> OnWebhookAsync(string pluginId, string version, CancellationToken ct = default);

    /// <summary>
    /// Downloads and extracts the specified version to a staging directory.
    /// Emits <see cref="HotReloadEvents.VersionStaged"/> on success.
    /// SPEC2050 US05 SC04.
    /// </summary>
    Task<bool> StageAsync(string pluginId, string version, CancellationToken ct = default);

    /// <summary>
    /// Executes the suspend → swap → resume handshake for the staged version of <paramref name="pluginId"/>.
    /// On success emits <see cref="HotReloadEvents.UpdateApplied"/>.
    /// On failure rolls back to the previous ALC and emits <see cref="HotReloadEvents.UpdateFailed"/>.
    /// SPEC2050 US05 SC05 / SC07 / SC09.
    /// </summary>
    Task<bool> ApplyAsync(string pluginId, CancellationToken ct = default);

    /// <summary>
    /// Explicitly rolls back to the previous version after a failed swap.
    /// SPEC2050 US05 SC07 / SC08 / SC09.
    /// </summary>
    Task RollbackAsync(string pluginId, string reason, CancellationToken ct = default);
}
