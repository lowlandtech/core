namespace LowlandTech.Core.Graph.EdgeTypes;

/// <summary>
/// EdgeType descriptor for the <c>List → Item</c> membership relationship.
/// A list node can reference any listable node through this edge.
/// </summary>
public class ListContainsItemEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.ListContainsItem.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "list.contains";
    public string Label => "Contains";
    public Guid SourceNodeTypeId => NodeTypes.ListNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => NodeTypes.ListNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["verb"] = "create",
        ["cardinality"] = "many",
        ["direction"] = "outgoing",
        ["ui.surface"] = "command-bar",
        ["ui.label"] = "Contains",
    };
}
