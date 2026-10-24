namespace LowlandTech.Core.Graph.NodeTypes;

public class AssigneeNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Assignee.Id;

    public Guid Id => NodeTypeId;
    public string Key => "assignee";
    public string Label => "Assignee";
    public string? Icon => nameof(Icons.User);
    public string? BlockComponent => null;
    public int SortOrder => 16;
    public Type? EntityType => null;
    public bool IsListable => true;
    public bool IsLeaf => true;
}
