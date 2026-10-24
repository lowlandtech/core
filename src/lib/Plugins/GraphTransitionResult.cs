namespace LowlandTech.Core.Plugins;

/// <summary>
/// Outcome of <see cref="IGraphEdgeTool{TSource,TTarget}.TryTransitionAsync"/>.
/// SPEC2050 US04.
/// </summary>
public sealed record GraphTransitionResult
{
    public required bool Success { get; init; }

    /// <summary>The phase that produced the failure when <see cref="Success"/> is false.</summary>
    public TransitionPhase? FailedPhase { get; init; }

    /// <summary>Human-readable failure reason. Null when <see cref="Success"/> is true.</summary>
    public string? FailureReason { get; init; }

    /// <summary>The target node produced by a successful reduce + effects pipeline.</summary>
    public object? Target { get; init; }

    /// <summary>Effects executed during the run, in order.</summary>
    public IReadOnlyList<EffectInvocation> Effects { get; init; } = [];

    /// <summary>True if the failure path executed a rollback over previously-applied effects.</summary>
    public bool RolledBack { get; init; }

    public static GraphTransitionResult Succeeded(object? target, IReadOnlyList<EffectInvocation> effects) =>
        new() { Success = true, Target = target, Effects = effects };

    public static GraphTransitionResult Failed(
        TransitionPhase phase,
        string reason,
        IReadOnlyList<EffectInvocation>? effects = null,
        bool rolledBack = false) =>
        new()
        {
            Success = false,
            FailedPhase = phase,
            FailureReason = reason,
            Effects = effects ?? [],
            RolledBack = rolledBack,
        };
}
