namespace LowlandTech.Core.Plugins;

/// <summary>
/// Context passed to <see cref="IPluginSeed.ApplyAsync"/>.
/// </summary>
public interface IPluginSeedContext
{
    IServiceProvider Services { get; }
    string PluginId { get; }
    Version FromVersion { get; }
    Version ToVersion { get; }
    bool IsFreshInstall { get; }
}
