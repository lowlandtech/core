namespace LowlandTech.Core.Graph.NodeTypes;

public class ActionNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Action.Id;

    public Guid Id => NodeTypeId;
    public string Key => "action";
    public string Label => "Action";
    public string? Icon => nameof(Icons.Run);
    public string? BlockComponent => "ActionBlock";
    public int SortOrder => 203;
    public Type? EntityType => null;
    public bool IsListable => true;
    public bool IsLeaf => true;
}
