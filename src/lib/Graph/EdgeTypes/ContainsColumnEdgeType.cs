
namespace LowlandTech.Core.Graph.EdgeTypes;

public class ContainsColumnEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.ContainsColumn.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "contains.column";
    public string Label => "Contains Column";
    public Guid SourceNodeTypeId => TableNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => ColumnNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>();
}
