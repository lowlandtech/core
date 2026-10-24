
namespace LowlandTech.Core.Plugins;

/// <summary>
/// In-process <see cref="IDomainEventBus"/> implementation. Dispatches each emitted
/// event to every subscriber registered for its runtime type, and keeps a bounded
/// capture log so tests can assert on <c>@postevent:</c> tags without hand-rolling a
/// recording bus per scenario.
/// SPEC2050 — template companion to <see cref="IDomainEventBus"/>.
/// </summary>
public sealed class DomainEventBus : IDomainEventBus
{
    private readonly ConcurrentDictionary<Type, List<Func<object, CancellationToken, Task>>> _handlers = new();
    private readonly List<object> _captured = [];
    private readonly object _captureLock = new();

    /// <summary>Every event emitted through this bus, in arrival order.</summary>
    public IReadOnlyList<object> Captured
    {
        get { lock (_captureLock) return _captured.ToArray(); }
    }

    /// <summary>All captured events of type <typeparamref name="TEvent"/> in arrival order.</summary>
    public IReadOnlyList<TEvent> CapturedOf<TEvent>() where TEvent : notnull
    {
        lock (_captureLock) return _captured.OfType<TEvent>().ToArray();
    }

    /// <summary>Subscribe to events of type <typeparamref name="TEvent"/>.</summary>
    public void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : notnull
    {
        ArgumentNullException.ThrowIfNull(handler);
        var list = _handlers.GetOrAdd(typeof(TEvent), _ => []);
        lock (list) list.Add((o, ct) => handler((TEvent)o, ct));
    }

    public async Task EmitAsync<TEvent>(TEvent payload, CancellationToken ct = default)
        where TEvent : notnull
    {
        ArgumentNullException.ThrowIfNull(payload);

        lock (_captureLock) _captured.Add(payload);

        if (!_handlers.TryGetValue(typeof(TEvent), out var list)) return;
        Func<object, CancellationToken, Task>[] snapshot;
        lock (list) snapshot = list.ToArray();
        foreach (var handler in snapshot)
            await handler(payload, ct).ConfigureAwait(false);
    }
}
