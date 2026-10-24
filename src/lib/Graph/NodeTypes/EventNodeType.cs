namespace LowlandTech.Core.Graph.NodeTypes;

public class EventNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Event.Id;

    public Guid Id => NodeTypeId;
    public string Key => "event";
    public string Label => "Event";
    public string? Icon => nameof(Icons.Bell);
    public string? BlockComponent => "EventBlock";
    public int SortOrder => 204;
    public Type? EntityType => null;
    public bool IsListable => true;
    public bool IsLeaf => true;
}
