namespace LowlandTech.Core.Plugins;

/// <summary>
/// Default <see cref="IPluginSeedContext"/> implementation.
/// </summary>
public sealed class PluginSeedContext(
    IServiceProvider services,
    string pluginId,
    Version fromVersion,
    Version toVersion) : IPluginSeedContext
{
    public IServiceProvider Services { get; } = services;
    public string PluginId { get; } = pluginId;
    public Version FromVersion { get; } = fromVersion;
    public Version ToVersion { get; } = toVersion;
    public bool IsFreshInstall => FromVersion == new Version(0, 0, 0);
}
