namespace LowlandTech.Core.Graph;

public record EdgeTypeEntry(
    Guid Id,
    string Key,
    string Label,
    Guid SourceNodeTypeId,
    Guid TargetNodeTypeId,
    string? ClrType,
    IReadOnlyDictionary<string, string> DefaultProperties,
    Type DescriptorType,
    string? NavigationPropertyName = null,
    string? Cardinality = null,
    bool IsStatic = true);

public interface IEdgeTypeRegistry
{
    IReadOnlyList<EdgeTypeEntry> All { get; }
    EdgeTypeEntry? FindByKey(string key);
    EdgeTypeEntry? FindById(Guid id);

    /// <summary>
    /// Find all EdgeTypes where the source is the given NodeType and the "required" property is "true".
    /// </summary>
    IReadOnlyList<EdgeTypeEntry> FindRequired(Guid sourceNodeTypeId);

    /// <summary>
    /// Find all EdgeTypes whose source node type matches <paramref name="sourceNodeTypeId"/>.
    /// </summary>
    IReadOnlyList<EdgeTypeEntry> FindBySourceNodeTypeId(Guid sourceNodeTypeId);

    /// <summary>
    /// Find all EdgeTypes whose target node type matches <paramref name="targetNodeTypeId"/>.
    /// </summary>
    IReadOnlyList<EdgeTypeEntry> FindByTargetNodeTypeId(Guid targetNodeTypeId);
}
