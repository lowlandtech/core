
namespace LowlandTech.Core.Graph.NodeTypes;

public class StepNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Step.Id;

    public Guid Id => NodeTypeId;
    public string Key => "step";
    public string Label => "Step";
    public string? Icon => nameof(Icons.Code);
    public string? BlockComponent => "StepBlock";
    public int SortOrder => 13;
    public Type? EntityType => typeof(Step);
    public bool IsListable => true;

    public EntityParentRef ExtractParent(object entity)
        => entity is Step step ? new EntityParentRef("flow", step.FlowId) : EntityParentRef.None;
}
