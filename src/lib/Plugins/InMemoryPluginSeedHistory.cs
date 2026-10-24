namespace LowlandTech.Core.Plugins;

/// <summary>
/// Default in-memory <see cref="IPluginSeedHistory"/> useful for tests and local dev.
/// Production hosts should swap in a database-backed implementation.
/// </summary>
public sealed class InMemoryPluginSeedHistory : IPluginSeedHistory
{
    private readonly List<SeedExecutionRecord> _records = [];
    private readonly object _lock = new();

    public Task<bool> HasAppliedAsync(string pluginId, string seedId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var found = _records.Any(r =>
                string.Equals(r.PluginId, pluginId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(r.SeedId, seedId, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(found);
        }
    }

    public Task RecordAsync(SeedExecutionRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        lock (_lock) _records.Add(record);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<SeedExecutionRecord>> ListAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            IReadOnlyList<SeedExecutionRecord> snapshot = _records
                .Where(r => string.Equals(r.PluginId, pluginId, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            return Task.FromResult(snapshot);
        }
    }

    public Task RemoveAllForPluginAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _records.RemoveAll(r => string.Equals(r.PluginId, pluginId, StringComparison.OrdinalIgnoreCase));
        }
        return Task.CompletedTask;
    }
}
