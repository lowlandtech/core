namespace LowlandTech.Core.Plugins;

/// <summary>
/// Source of plugin packages available for install. Production hosts back this with
/// a NuGet V3 feed; tests and local dev can use <see cref="InMemoryPluginFeed"/>.
/// SPEC2050 US08.
/// </summary>
public interface IPluginFeed
{
    Task<IReadOnlyList<FeedEntry>> ListAsync(CancellationToken cancellationToken = default);
}

public sealed record FeedEntry(string Id, string Name, string Version);

public sealed class InMemoryPluginFeed : IPluginFeed
{
    private readonly List<FeedEntry> _entries = [];
    private readonly object _lock = new();

    public void Add(FeedEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        lock (_lock) _entries.Add(entry);
    }

    public Task<IReadOnlyList<FeedEntry>> ListAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            IReadOnlyList<FeedEntry> snapshot = _entries.ToArray();
            return Task.FromResult(snapshot);
        }
    }
}
