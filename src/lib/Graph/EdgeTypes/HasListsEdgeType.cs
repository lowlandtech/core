namespace LowlandTech.Core.Graph.EdgeTypes;

/// <summary>
/// EdgeType descriptor for the <c>UserContext → Lists</c> relationship.
/// Required so every user-context exposes a Lists navigation header.
/// </summary>
public class HasListsEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasLists.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.lists";
    public string Label => "Has Lists";
    public Guid SourceNodeTypeId => UserContextNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => ListsHeaderNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["required"] = "false",
        ["label"] = "Lists",
        ["icon"] = nameof(Icons.List),
        ["cascadeDelete"] = "true",
    };
}
