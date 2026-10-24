
namespace LowlandTech.Core.Graph.EdgeTypes;

public class ContainsWorkflowEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.ContainsWorkflow.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "contains.workflow";
    public string Label => "Contains Workflow";
    public Guid SourceNodeTypeId => WorkspaceNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => WorkflowNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>();
}
