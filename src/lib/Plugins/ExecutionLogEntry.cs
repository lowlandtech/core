namespace LowlandTech.Core.Plugins;

/// <summary>
/// One line of the execution log written by an <see cref="IGraphEdgeTool{TSource,TTarget}"/>.
/// SPEC2050 US04.
/// </summary>
public sealed record ExecutionLogEntry(
    DateTimeOffset Timestamp,
    TransitionPhase Phase,
    string Action,
    IReadOnlyDictionary<string, object?> Parameters,
    bool Succeeded,
    string? Error = null);
