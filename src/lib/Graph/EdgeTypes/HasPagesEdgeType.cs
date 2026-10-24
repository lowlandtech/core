
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasPagesEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasPages.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.pages";
    public string Label => "Has Pages";
    public Guid SourceNodeTypeId => WorkspaceNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => HeaderNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["required"] = "true",
        ["label"] = "Pages",
        ["icon"] = nameof(Icons.Pages),
        ["cascadeDelete"] = "true",
    };
}
