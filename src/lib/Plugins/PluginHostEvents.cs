namespace LowlandTech.Core.Plugins;

/// <summary>
/// Emitted after <see cref="IPluginHostService.LoadPluginsAsync"/> finishes
/// invoking every discovered plugin's boot lifecycle. <see cref="LoadedPluginIds"/>
/// is the dependency-ordered list of plugins the host brought online.
/// SPEC2050 US14 SC01 UAC05.
/// </summary>
public sealed record PluginsLoaded(IReadOnlyList<string> LoadedPluginIds);

/// <summary>
/// Emitted after <see cref="IPluginHostService.MakeAvailableAsync"/> when
/// <see cref="IEndpointGroupRegistry"/> contains endpoint groups contributed
/// by the plugin. Lists the group IDs so the host can log, audit, or route-refresh.
/// SPEC2050 US08 SC11.
/// </summary>
public sealed record EndpointsRegistered(string PluginId, IReadOnlyList<string> EndpointGroupIds);
