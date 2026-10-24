
namespace LowlandTech.Core.Plugins.InstallModes;

/// <summary>
/// Installs plugins by swapping the plugin's <see cref="System.Runtime.Loader.AssemblyLoadContext"/>
/// in-process — delegates to <see cref="IHotReloadService"/> (US05.HotReloadService).
/// SPEC2050 US20 SC02 (happy path) + SC06 (resume-failure rollback).
/// </summary>
public sealed class HotReloadInstaller : IPluginInstaller
{
    private readonly IHotReloadService? _hotReload;
    private readonly IDomainEventBus? _eventBus;

    /// <summary>
    /// Parameterless constructor: supports <see cref="PlanAsync"/> only.
    /// Use the full constructor when <see cref="ExecuteAsync"/> is required.
    /// </summary>
    public HotReloadInstaller() { }

    public HotReloadInstaller(IHotReloadService hotReload, IDomainEventBus eventBus)
    {
        _hotReload = hotReload;
        _eventBus = eventBus;
    }

    public PluginInstallMode Mode => PluginInstallMode.HotReload;

    public Task<InstallPlan> PlanAsync(PluginRef package, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(package);

        var steps = new[]
        {
            new InstallStep("suspend old ALC",
                "Quiesce the currently-loaded plugin instance via IApiPlugin.SuspendAsync so no requests are in flight when the ALC is swapped."),
            new InstallStep("swap ALC",
                "Load the new plugin assembly into a fresh AssemblyLoadContext and unload the previous ALC."),
            new InstallStep("resume new ALC",
                "Restart the plugin via IApiPlugin.ResumeAsync on the newly-loaded ALC.")
        };

        var plan = new InstallPlan(PluginInstallMode.HotReload, package, steps);
        return Task.FromResult(plan);
    }

    /// <summary>
    /// Delegates to <see cref="IHotReloadService"/>:
    ///   1. <see cref="IHotReloadService.StageAsync"/> — download and extract the package.
    ///   2. <see cref="IHotReloadService.ApplyAsync"/> — suspend → swap ALC → resume.
    /// On success emits <see cref="HotReloadInstalled"/>.
    /// On failure (SC06) <see cref="IHotReloadService"/> handles rollback internally and
    /// emits <see cref="UpdateFailed"/>; this method additionally emits
    /// <see cref="InstallRolledBack"/>.
    /// SPEC2050 US20 SC02 / SC06.
    /// </summary>
    public async Task<InstallResult> ExecuteAsync(InstallPlan plan, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(plan);

        if (_hotReload is null || _eventBus is null)
            throw new InvalidOperationException(
                "HotReloadInstaller.ExecuteAsync requires IHotReloadService and IDomainEventBus. " +
                "Use the full constructor.");

        // Stage the new version (download + extract).
        var staged = await _hotReload.StageAsync(plan.Package.PluginId, plan.Package.Version, ct)
            .ConfigureAwait(false);

        if (!staged)
        {
            var reason = "staging-failed";
            await _eventBus.EmitAsync(new InstallRolledBack(plan.Package.PluginId, reason), ct)
                .ConfigureAwait(false);
            return new InstallResult(Succeeded: false, Error: reason);
        }

        // Apply: suspend old ALC → swap → resume new ALC.
        var ok = await _hotReload.ApplyAsync(plan.Package.PluginId, ct).ConfigureAwait(false);

        if (!ok)
        {
            var reason = "hot-reload apply failed";
            await _eventBus.EmitAsync(new InstallRolledBack(plan.Package.PluginId, reason), ct)
                .ConfigureAwait(false);
            return new InstallResult(Succeeded: false, Error: reason);
        }

        // Read session to get ALC names for the event.
        var session = _hotReload.GetSession(plan.Package.PluginId);
        var oldAlcName = session.OldAlcName ?? string.Empty;
        var newAlcName = session.NewAlcName ?? $"{plan.Package.PluginId}@{plan.Package.Version}";

        await _eventBus.EmitAsync(new HotReloadInstalled(plan.Package.PluginId, oldAlcName, newAlcName), ct)
            .ConfigureAwait(false);

        return new InstallResult(Succeeded: true, NewAlcName: newAlcName);
    }
}
