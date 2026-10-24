namespace LowlandTech.Core.Graph.NodeTypes;

public class UserContextNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.UserContext.Id;

    public Guid Id => NodeTypeId;
    public string Key => "user-context";
    public string Label => "User Context";
    public string? Icon => null;
    public string? BlockComponent => null;
    public int SortOrder => 0;
    public Type? EntityType => null;
}
