
namespace LowlandTech.Core.Graph.NodeTypes;

public class FlowNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Flow.Id;

    public Guid Id => NodeTypeId;
    public string Key => "flow";
    public string Label => "Flow";
    public string? Icon => nameof(Icons.Workflow);
    public string? BlockComponent => "FlowBlock";
    public int SortOrder => 12;
    public Type? EntityType => typeof(Flow);
    public bool IsListable => true;
    public IReadOnlyList<NavigationHeaderDefinition>? NavigationHeaders =>
    [
        new("Steps", "Steps", "step", 0, nameof(Icons.Code))
    ];
}
