
namespace LowlandTech.Core.Graph.NodeTypes;

public class WorkflowNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Workflow.Id;

    public Guid Id => NodeTypeId;
    public string Key => "workflow";
    public string Label => "Workflow";
    public string? Icon => nameof(Icons.Workflow);
    public string? BlockComponent => "WorkflowBlock";
    public int SortOrder => 5;
    public Type? EntityType => typeof(Workflow);
    public bool IsListable => true;

    public EntityParentRef ExtractParent(object entity)
        => entity is Workflow wf ? new EntityParentRef("workspace", wf.WorkspaceId) : EntityParentRef.None;
}
