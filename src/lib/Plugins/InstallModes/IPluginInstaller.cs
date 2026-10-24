namespace LowlandTech.Core.Plugins.InstallModes;

/// <summary>
/// Strategy that plans and executes plugin installation for a single
/// <see cref="PluginInstallMode"/>. The host resolves the concrete installer
/// by matching <see cref="Mode"/> against the plugin descriptor's
/// declared install modes.
/// SPEC2050 US20.
/// </summary>
public interface IPluginInstaller
{
    PluginInstallMode Mode { get; }

    Task<InstallPlan> PlanAsync(PluginRef package, CancellationToken ct);

    Task<InstallResult> ExecuteAsync(InstallPlan plan, CancellationToken ct);
}
