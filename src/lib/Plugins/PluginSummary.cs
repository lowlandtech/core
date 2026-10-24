namespace LowlandTech.Core.Plugins;

/// <summary>
/// One row in <see cref="IPluginHostService.DisplayAsync"/> output: identity + state.
/// SPEC2050 US08.
/// </summary>
public sealed record PluginSummary(
    string Id,
    string Name,
    string InstalledVersion,
    PluginInstallState State,
    string? AvailableVersion = null);
