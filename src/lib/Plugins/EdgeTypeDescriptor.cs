namespace LowlandTech.Core.Plugins;

/// <summary>
/// Declarative description of a noun-verb edge type contributed by a feature.
/// SPEC2050 US04.
/// </summary>
/// <remarks>
/// Distinct from <see cref="LowlandTech.Core.Graph.EdgeTypeEntry"/>: that one belongs to
/// the graph metadata layer (Guid-based node type ids); this one is the plugin-side
/// noun-verb capability binding that resolves to an <see cref="IGraphEdgeTool{TSource,TTarget}"/>
/// implementation through the feature system.
/// </remarks>
public sealed record EdgeTypeDescriptor
{
    public required string Verb { get; init; }
    public required string SourceNodeType { get; init; }
    public required string TargetNodeType { get; init; }

    /// <summary>Concrete tool type that implements <see cref="IGraphEdgeTool{TSource,TTarget}"/>.</summary>
    public required Type ToolType { get; init; }

    /// <summary>Feature that owns this edge type. Used for enabled-state filtering.</summary>
    public required string FeatureId { get; init; }
}
