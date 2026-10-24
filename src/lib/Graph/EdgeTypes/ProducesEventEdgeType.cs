
namespace LowlandTech.Core.Graph.EdgeTypes;

public class ProducesEventEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.ProducesEvent.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "produces.event";
    public string Label => "Produces Event";
    public Guid SourceNodeTypeId => ActionNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => EventNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["icon"] = nameof(Icons.Bell),
    };
}
