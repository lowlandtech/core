namespace LowlandTech.Core.Graph.NodeTypes;

/// <summary>
/// Typed header descriptor for the <c>Steps</c> navigation group inside a Flow.
/// <c>IsHeader = true</c> lets <c>ResolveHeaderEdgeTypes</c> (strategy 3) find this
/// descriptor by label and return the <see cref="StepsHeaderHasStepEdgeType"/>
/// create-edge so the dashboard command bar shows "Add Step" when Steps is selected.
/// </summary>
public class StepsHeaderNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.StepsHeader.Id;

    public Guid    Id             => NodeTypeId;
    public string  Key            => "step.header";
    public string  Label          => "Steps";
    public string? Icon           => nameof(Icons.Code);
    public string? BlockComponent => null;
    public int     SortOrder      => 0;
    public Type?   EntityType     => null;
    public bool    IsHeader       => true;
}
