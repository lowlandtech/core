namespace LowlandTech.Core.Plugins;

/// <summary>
/// Context passed to <see cref="IApiPlugin.InstallAsync"/> and <see cref="IApiPlugin.UninstallAsync"/>.
/// </summary>
public interface IPluginInstallContext
{
    IServiceProvider Services { get; }
    string PluginId { get; }
}
