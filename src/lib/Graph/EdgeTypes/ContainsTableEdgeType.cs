
namespace LowlandTech.Core.Graph.EdgeTypes;

public class ContainsTableEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.ContainsTable.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "contains.table";
    public string Label => "Contains Table";
    public Guid SourceNodeTypeId => WorkspaceNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => TableNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>();
}
