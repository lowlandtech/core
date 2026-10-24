namespace LowlandTech.Core.Graph;

public interface IEdgeTypeDescriptor
{
    Guid Id { get; }
    string Key { get; }
    string Label { get; }
    Guid SourceNodeTypeId { get; }
    Guid TargetNodeTypeId { get; }
    string? ClrType { get; }

    /// <summary>
    /// Default properties for this EdgeType (e.g. { "required", "true" }).
    /// Applied when the EdgeType row is seeded.
    /// </summary>
    IReadOnlyDictionary<string, string> DefaultProperties { get; }

    /// <summary>
    /// Optional intent of the edge (e.g. "create", "navigate"). Derived from
    /// <see cref="DefaultProperties"/>["verb"] by default so existing descriptors
    /// do not need to change.
    /// </summary>
    string? Intent => DefaultProperties.TryGetValue("verb", out var value) ? value : null;

    /// <summary>
    /// Optional tool key used to resolve a graph tool/service that can materialise
    /// the target node. Derived from <see cref="DefaultProperties"/>["tool"].
    /// </summary>
    string? Tool => DefaultProperties.TryGetValue("tool", out var value) ? value : null;
}
