namespace LowlandTech.Core.Plugins;

/// <summary>
/// Non-generic adapter so a <see cref="GraphBranch"/> can invoke any
/// <see cref="IGraphEdgeTool{TSource,TTarget}"/> via an untyped <c>object</c> source.
/// SPEC2050 US05.
/// </summary>
public interface IGraphEdgeToolAdapter
{
    string Verb { get; }

    Task<GraphTransitionResult> InvokeAsync(
        object source,
        TransitionContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Typed wrapper that casts the <c>object</c> source to <typeparamref name="TSource"/>
/// before forwarding to the underlying typed tool.
/// </summary>
public sealed class GraphEdgeToolAdapter<TSource, TTarget>(IGraphEdgeTool<TSource, TTarget> tool)
    : IGraphEdgeToolAdapter
    where TSource : class
    where TTarget : class
{
    public string Verb => tool.Verb;

    public Task<GraphTransitionResult> InvokeAsync(
        object source,
        TransitionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        var typed = source as TSource
            ?? throw new InvalidCastException(
                $"Expected source of type {typeof(TSource).Name}; got {source.GetType().Name}.");
        return tool.TryTransitionAsync(typed, context, cancellationToken);
    }
}
