namespace LowlandTech.Core.Graph.NodeTypes;

public class SessionNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Session.Id;

    public Guid Id => NodeTypeId;
    public string Key => "session";
    public string Label => "Session";
    public string? Icon => nameof(Icons.MessageCircle);
    public string? BlockComponent => null;
    public int SortOrder => 2;
    public Type? EntityType => null;
    public bool IsListable => true;
    public bool IsLeaf => true;
}
