
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasAssigneeEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasAssignee.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.assignee";
    public string Label => "Has Assignee";
    public Guid SourceNodeTypeId => SessionNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => AssigneeNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["label"] = "Assignee",
        ["icon"] = nameof(Icons.User),
        ["multiplicity"] = "*",
        ["cascadeDelete"] = "true",
    };
}
