namespace LowlandTech.Core.Graph.NodeTypes;

/// <summary>
/// Descriptor for an API key managed by the SessionBlock settings surface.
/// </summary>
public class SessionApiKeyNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.SessionApiKey.Id;

    public Guid Id => NodeTypeId;
    public string Key => "session-api-key";
    public string Label => "API Key";
    public string? Icon => nameof(Icons.Key);
    public string? BlockComponent => "NodeBlock";
    public int SortOrder => 0;
    public Type? EntityType => null;
    public bool IsLeaf => true;
}
