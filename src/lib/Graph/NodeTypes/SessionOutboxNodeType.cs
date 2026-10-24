namespace LowlandTech.Core.Graph.NodeTypes;

/// <summary>
/// Descriptor for the SessionBlock outbox queue projection.
/// </summary>
public class SessionOutboxNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.SessionOutbox.Id;

    public Guid Id => NodeTypeId;
    public string Key => "session-outbox";
    public string Label => "Outbox";
    public string? Icon => nameof(Icons.Inbox);
    public string? BlockComponent => "NodeBlock";
    public int SortOrder => 2;
    public Type? EntityType => null;
    public bool IsLeaf => true;
}
