namespace LowlandTech.Core.Graph.NodeTypes;

public class MessageNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Message.Id;

    public Guid Id => NodeTypeId;
    public string Key => "message";
    public string Label => "Message";
    public string? Icon => null;
    public string? BlockComponent => null;
    public int SortOrder => 10;
    public Type? EntityType => null;
    public bool IsListable => true;
    public bool IsLeaf => true;
}
