
namespace LowlandTech.Core.Graph.EdgeTypes;

public class ContainsPageItemEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.ContainsPageItem.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "contains.page-item";
    public string Label => "Contains Block";
    public Guid SourceNodeTypeId => PageItemNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => PageItemNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["required"] = "false",
        ["label"] = "Block",
        ["icon"] = nameof(Icons.PageItem),
        ["cascadeDelete"] = "true",
    };
}
