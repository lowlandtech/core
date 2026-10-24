
namespace LowlandTech.Core.Plugins;

/// <summary>
/// Top-level orchestrator that runs the plugin lifecycle pipeline:
/// Display → Install → MakeAvailable → Suspend / Resume → Uninstall.
/// SPEC2050 US08.
/// </summary>
public interface IPluginHostService
{
    /// <summary>
    /// Discover installed plugins, sort them by dependency order (US14), and
    /// invoke <see cref="IApiPlugin.ConfigureServices"/> on each before
    /// <c>app.Build()</c>. Runs once for <c>routed</c> / <c>hotReload</c> hosts;
    /// sidecar plugins are excluded because their lifecycle runs out-of-process.
    /// SPEC2050 US14.
    /// </summary>
    Task LoadPluginsAsync(IServiceCollection services, CancellationToken cancellationToken = default);

    /// <summary>
    /// Join the configured feed with the local installed registry. Plugins present in both
    /// are reported with state <see cref="PluginInstallState.Installed"/> (or
    /// <see cref="PluginInstallState.Suspended"/>); plugins only in the feed are reported
    /// as <see cref="PluginInstallState.Available"/>.
    /// </summary>
    Task<IReadOnlyList<PluginSummary>> DisplayAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Install a plugin: validate dependencies → register descriptor → ConfigureServices →
    /// run schema migrations → seed → MakeAvailable.
    /// </summary>
    Task<PluginInstallResult> InstallAsync(IApiPlugin plugin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Patch the live registries with descriptors contributed by the plugin's feature
    /// packages. Idempotent if already made available.
    /// </summary>
    Task MakeAvailableAsync(string pluginId, IEnumerable<IFeaturePackage> features, CancellationToken cancellationToken = default);

    /// <summary>
    /// Quiesce a plugin for hot-reload: stop its background jobs and mark it suspended.
    /// </summary>
    Task SuspendAsync(string pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restart a previously-suspended plugin's background jobs.
    /// </summary>
    Task ResumeAsync(string pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tear down a plugin: refuse if dependents are installed, otherwise remove
    /// every descriptor (features, jobs, events, MCP tools, edge types, pages, blocks,
    /// permissions, resources, contexts, seed history) and unregister the plugin.
    /// </summary>
    Task<PluginUninstallResult> UninstallAsync(string pluginId, CancellationToken cancellationToken = default);
}
