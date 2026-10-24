namespace LowlandTech.Core.Plugins;

/// <summary>
/// Persistent record of which seeds have been applied for which plugins.
/// SPEC2050 US15.
/// </summary>
/// <remarks>
/// History lives in the host's persistent store, NOT in any plugin schema, so it
/// survives plugin uninstall (the host can purge entries on uninstall).
/// </remarks>
public interface IPluginSeedHistory
{
    Task<bool> HasAppliedAsync(string pluginId, string seedId, CancellationToken cancellationToken = default);
    Task RecordAsync(SeedExecutionRecord record, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SeedExecutionRecord>> ListAsync(string pluginId, CancellationToken cancellationToken = default);
    Task RemoveAllForPluginAsync(string pluginId, CancellationToken cancellationToken = default);
}
