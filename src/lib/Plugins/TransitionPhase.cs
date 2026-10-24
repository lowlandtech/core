namespace LowlandTech.Core.Plugins;

/// <summary>
/// The four phases an <see cref="IGraphEdgeTool{TSource,TTarget}"/> walks through.
/// SPEC2050 US04.
/// </summary>
public enum TransitionPhase
{
    /// <summary>State assertions checked before any reduce or effect runs.</summary>
    Precondition = 1,

    /// <summary>Pure state transition. Must be deterministic and side-effect free.</summary>
    Reduce = 2,

    /// <summary>Declared side effects (DB writes, HTTP calls, …) executed and logged.</summary>
    Effect = 3,

    /// <summary>State assertions checked after the reduce + effects. Failure triggers rollback.</summary>
    Postcondition = 4,
}
