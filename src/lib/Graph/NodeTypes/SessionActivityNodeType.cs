namespace LowlandTech.Core.Graph.NodeTypes;

/// <summary>
/// Descriptor for SessionBlock settings activity/event-log projection.
/// </summary>
public class SessionActivityNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.SessionActivity.Id;

    public Guid Id => NodeTypeId;
    public string Key => "session-activity";
    public string Label => "Activity";
    public string? Icon => nameof(Icons.Clock);
    public string? BlockComponent => "NodeBlock";
    public int SortOrder => 1;
    public Type? EntityType => null;
    public bool IsLeaf => true;
}
