namespace LowlandTech.Core.Plugins.InstallModes;

/// <summary>
/// Raised by <see cref="IPluginInstaller.PlanAsync"/> when the host cannot install
/// a plugin in any mode the plugin declares — i.e. the intersection of the host's
/// supported modes and the plugin's declared modes is empty.
/// SPEC2050 US20 SC04.
/// </summary>
public sealed class InstallModeNotSupportedException(
    string pluginId,
    IReadOnlyList<PluginInstallMode> hostSupported,
    IReadOnlyList<PluginInstallMode> pluginDeclared)
    : InvalidOperationException(
        $"Plugin '{pluginId}' cannot be installed. " +
        $"Host supports: [{string.Join(", ", hostSupported)}]. " +
        $"Plugin declares: [{string.Join(", ", pluginDeclared)}].")
{
    public string PluginId { get; } = pluginId;
    public IReadOnlyList<PluginInstallMode> HostSupportedModes { get; } = hostSupported;
    public IReadOnlyList<PluginInstallMode> PluginDeclaredModes { get; } = pluginDeclared;
}
