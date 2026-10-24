namespace LowlandTech.Core.Plugins;

/// <summary>
/// Append-only audit log written by <see cref="IGraphEdgeTool{TSource,TTarget}"/> instances
/// during a transition.
/// SPEC2050 US04.
/// </summary>
public interface IExecutionLog
{
    IReadOnlyList<ExecutionLogEntry> Entries { get; }
    void Append(ExecutionLogEntry entry);
}

/// <summary>Default in-memory <see cref="IExecutionLog"/> useful for tests and short-lived transitions.</summary>
public sealed class InMemoryExecutionLog : IExecutionLog
{
    private readonly List<ExecutionLogEntry> _entries = [];
    private readonly object _lock = new();

    public IReadOnlyList<ExecutionLogEntry> Entries
    {
        get { lock (_lock) return _entries.ToArray(); }
    }

    public void Append(ExecutionLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        lock (_lock) _entries.Add(entry);
    }
}
