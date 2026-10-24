
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasPageEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasPage.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.page";
    public string Label => "Has Page";
    public Guid SourceNodeTypeId => HeaderNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => PageNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["required"] = "false",
        ["label"] = "Page",
        ["icon"] = nameof(Icons.Page),
    };
}
