
namespace LowlandTech.Core.Graph.EdgeTypes;

public class ReactsToEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.ReactsTo.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "reacts.to";
    public string Label => "Reacts To";
    public Guid SourceNodeTypeId => HandlerNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => EventNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["icon"] = nameof(Icons.ServiceProcessor),
    };
}
