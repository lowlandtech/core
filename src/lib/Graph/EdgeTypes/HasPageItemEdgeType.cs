
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasPageItemEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasPageItem.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.page-item";
    public string Label => "Has Page Block";
    public Guid SourceNodeTypeId => HeaderNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => PageItemNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["required"] = "false",
        ["label"] = "Block",
        ["icon"] = nameof(Icons.PageItem),
    };
}
