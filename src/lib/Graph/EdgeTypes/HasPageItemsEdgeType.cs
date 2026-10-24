
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasPageItemsEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasPageItems.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.page-items";
    public string Label => "Has Blocks";
    public Guid SourceNodeTypeId => PageNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => HeaderNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["required"] = "true",
        ["label"] = "Blocks",
        ["icon"] = nameof(Icons.PageBlock),
        ["cascadeDelete"] = "true",
    };
}
