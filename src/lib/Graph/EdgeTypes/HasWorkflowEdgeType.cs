
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasWorkflowEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasWorkflow.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.workflow";
    public string Label => "Has Workflow";
    public Guid SourceNodeTypeId => SessionNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => WorkflowNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["label"] = "Workflow",
        ["icon"] = nameof(Icons.Workflow),
        ["multiplicity"] = "1",
    };
}
