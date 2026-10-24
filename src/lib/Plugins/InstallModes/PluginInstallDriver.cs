
namespace LowlandTech.Core.Plugins.InstallModes;

/// <summary>
/// Orchestrates plugin installation by intersecting the host's supported modes
/// with the plugin's declared modes, then delegating to the appropriate
/// <see cref="IPluginInstaller"/>. Rejects at plan time when the intersection
/// is empty (SPEC2050 US20 SC04).
/// </summary>
public interface IPluginInstallDriver
{
    Task<InstallPlan> PlanAsync(PluginDescriptor descriptor, PluginRef package, CancellationToken ct);
    Task<InstallResult> ExecuteAsync(InstallPlan plan, CancellationToken ct);
}

/// <inheritdoc/>
public sealed class PluginInstallDriver : IPluginInstallDriver
{
    private readonly IReadOnlyList<IPluginInstaller> _installers;
    private readonly IReadOnlyList<PluginInstallMode> _hostSupportedModes;

    public PluginInstallDriver(
        IEnumerable<IPluginInstaller> installers,
        IEnumerable<PluginInstallMode> hostSupportedModes)
    {
        _installers = installers.ToArray();
        _hostSupportedModes = hostSupportedModes.ToArray();
    }

    public Task<InstallPlan> PlanAsync(PluginDescriptor descriptor, PluginRef package, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(package);

        var intersection = descriptor.InstallModes.Intersect(_hostSupportedModes).ToArray();
        if (intersection.Length == 0)
            throw new InstallModeNotSupportedException(descriptor.Id, _hostSupportedModes, descriptor.InstallModes);

        // Choose the first intersecting mode ordered by host preference (US20 SC07).
        var chosen = intersection[0];
        var installer = _installers.First(i => i.Mode == chosen);
        return installer.PlanAsync(package, ct);
    }

    public Task<InstallResult> ExecuteAsync(InstallPlan plan, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var installer = _installers.First(i => i.Mode == plan.Mode);
        return installer.ExecuteAsync(plan, ct);
    }
}
