namespace LowlandTech.Core.Plugins;

/// <summary>
/// Context handed to every phase of an <see cref="IGraphEdgeTool{TSource,TTarget}"/>.
/// SPEC2050 US04.
/// </summary>
public sealed class TransitionContext
{
    public TransitionContext(
        IReadOnlyDictionary<string, object?>? parameters = null,
        IExecutionLog? log = null)
    {
        Parameters = parameters ?? new Dictionary<string, object?>();
        Log = log ?? new InMemoryExecutionLog();
    }

    public IReadOnlyDictionary<string, object?> Parameters { get; }
    public IExecutionLog Log { get; }
}
