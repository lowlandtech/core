namespace LowlandTech.Core.Plugins;

/// <summary>
/// Noun-verb capability contract: a tool that transitions a source node into a target
/// node by executing a four-phase pipeline (Precondition → Reduce → Effect → Postcondition).
/// SPEC2050 US04.
/// </summary>
public interface IGraphEdgeTool<TSource, TTarget>
    where TSource : class
    where TTarget : class
{
    /// <summary>Edge verb e.g. <c>"create"</c>, <c>"approve"</c>, <c>"create.graph"</c>.</summary>
    string Verb { get; }

    /// <summary>Run the four-phase pipeline. Always returns a <see cref="GraphTransitionResult"/>; never throws on caller errors.</summary>
    Task<GraphTransitionResult> TryTransitionAsync(
        TSource source,
        TransitionContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Default base class implementing the four-phase pipeline. Subclasses override the
/// hooks for the phases they need; the base orchestrator handles logging, ordering,
/// rollback, and the <see cref="GraphTransitionResult"/> shape.
/// </summary>
public abstract class GraphEdgeTool<TSource, TTarget> : IGraphEdgeTool<TSource, TTarget>
    where TSource : class
    where TTarget : class
{
    public abstract string Verb { get; }

    public async Task<GraphTransitionResult> TryTransitionAsync(
        TSource source,
        TransitionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(context);

        var log = context.Log;
        var effects = new List<EffectInvocation>();

        // Phase 1: Precondition
        try
        {
            var ok = await CheckPreconditionAsync(source, context, cancellationToken).ConfigureAwait(false);
            log.Append(new ExecutionLogEntry(
                DateTimeOffset.UtcNow,
                TransitionPhase.Precondition,
                Verb,
                context.Parameters,
                ok,
                ok ? null : "Precondition returned false"));
            if (!ok)
                return GraphTransitionResult.Failed(TransitionPhase.Precondition, "Precondition returned false");
        }
        catch (Exception ex)
        {
            log.Append(new ExecutionLogEntry(
                DateTimeOffset.UtcNow,
                TransitionPhase.Precondition,
                Verb,
                context.Parameters,
                false,
                ex.Message));
            return GraphTransitionResult.Failed(TransitionPhase.Precondition, ex.Message);
        }

        // Phase 2: Reduce (pure)
        TTarget target;
        try
        {
            target = Reduce(source, context.Parameters);
            log.Append(new ExecutionLogEntry(
                DateTimeOffset.UtcNow,
                TransitionPhase.Reduce,
                Verb,
                context.Parameters,
                true));
        }
        catch (Exception ex)
        {
            log.Append(new ExecutionLogEntry(
                DateTimeOffset.UtcNow,
                TransitionPhase.Reduce,
                Verb,
                context.Parameters,
                false,
                ex.Message));
            return GraphTransitionResult.Failed(TransitionPhase.Reduce, ex.Message);
        }

        // Phase 3: Effects
        try
        {
            await ApplyEffectsAsync(source, target, context, effects, cancellationToken).ConfigureAwait(false);
            foreach (var effect in effects)
            {
                log.Append(new ExecutionLogEntry(
                    DateTimeOffset.UtcNow,
                    TransitionPhase.Effect,
                    effect.Name,
                    effect.Parameters,
                    effect.Succeeded,
                    effect.Error));
            }
        }
        catch (Exception ex)
        {
            log.Append(new ExecutionLogEntry(
                DateTimeOffset.UtcNow,
                TransitionPhase.Effect,
                Verb,
                context.Parameters,
                false,
                ex.Message));
            await RollbackAsync(effects, context, cancellationToken).ConfigureAwait(false);
            return GraphTransitionResult.Failed(TransitionPhase.Effect, ex.Message, effects, rolledBack: true);
        }

        // Phase 4: Postcondition
        try
        {
            var ok = await CheckPostconditionAsync(source, target, context, cancellationToken).ConfigureAwait(false);
            log.Append(new ExecutionLogEntry(
                DateTimeOffset.UtcNow,
                TransitionPhase.Postcondition,
                Verb,
                context.Parameters,
                ok,
                ok ? null : "Postcondition returned false"));
            if (!ok)
            {
                await RollbackAsync(effects, context, cancellationToken).ConfigureAwait(false);
                return GraphTransitionResult.Failed(
                    TransitionPhase.Postcondition,
                    "Postcondition returned false",
                    effects,
                    rolledBack: true);
            }
        }
        catch (Exception ex)
        {
            log.Append(new ExecutionLogEntry(
                DateTimeOffset.UtcNow,
                TransitionPhase.Postcondition,
                Verb,
                context.Parameters,
                false,
                ex.Message));
            await RollbackAsync(effects, context, cancellationToken).ConfigureAwait(false);
            return GraphTransitionResult.Failed(TransitionPhase.Postcondition, ex.Message, effects, rolledBack: true);
        }

        return GraphTransitionResult.Succeeded(target, effects);
    }

    /// <summary>Check that <paramref name="source"/> + <paramref name="context"/> are valid for this transition.</summary>
    protected virtual ValueTask<bool> CheckPreconditionAsync(
        TSource source,
        TransitionContext context,
        CancellationToken cancellationToken) => ValueTask.FromResult(true);

    /// <summary>
    /// Pure state transition. MUST be deterministic and free of IO/side effects.
    /// </summary>
    protected abstract TTarget Reduce(TSource source, IReadOnlyDictionary<string, object?> parameters);

    /// <summary>
    /// Apply declared side effects. Each effect should be appended to <paramref name="effects"/>
    /// so the orchestrator can log and roll them back if a later phase fails.
    /// </summary>
    protected virtual ValueTask ApplyEffectsAsync(
        TSource source,
        TTarget target,
        TransitionContext context,
        IList<EffectInvocation> effects,
        CancellationToken cancellationToken) => ValueTask.CompletedTask;

    /// <summary>Verify the resulting state is consistent. Failure triggers rollback.</summary>
    protected virtual ValueTask<bool> CheckPostconditionAsync(
        TSource source,
        TTarget target,
        TransitionContext context,
        CancellationToken cancellationToken) => ValueTask.FromResult(true);

    /// <summary>Reverse previously-applied effects when a later phase fails. Default: no-op.</summary>
    protected virtual ValueTask RollbackAsync(
        IReadOnlyList<EffectInvocation> effects,
        TransitionContext context,
        CancellationToken cancellationToken) => ValueTask.CompletedTask;
}
