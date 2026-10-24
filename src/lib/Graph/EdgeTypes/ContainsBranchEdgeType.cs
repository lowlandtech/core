
namespace LowlandTech.Core.Graph.EdgeTypes;

public class ContainsBranchEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.ContainsBranch.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "contains.branch";
    public string Label => "Contains Branch";
    public Guid SourceNodeTypeId => WorkspaceNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => BranchNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>();
}
