namespace LowlandTech.Core.Graph.EdgeTypes;

/// <summary>
/// EdgeType descriptor for the <c>Lists → List</c> create-relationship.
/// verb="create": when the Lists header is selected the dashboard command bar
/// binds to this edge and presents "Create List" as the primary create intent.
/// </summary>
public class HasListEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasList.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.list";
    public string Label => "Create List";
    public Guid SourceNodeTypeId => ListsHeaderNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => ListNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["verb"] = "create",
        ["cardinality"] = "many",
        ["direction"] = "outgoing",
        ["ui.surface"] = "command-bar",
        ["ui.label"] = "Create List",
    };
}
