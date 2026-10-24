
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasSessionsEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasSessions.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.sessions";
    public string Label => "Has Sessions";
    public Guid SourceNodeTypeId => UserContextNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => HeaderNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["required"] = "false",
        ["label"] = "Sessions",
        ["icon"] = nameof(Icons.MessageCircle),
        ["cascadeDelete"] = "true",
    };
}
