
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasMessageEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasMessage.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.message";
    public string Label => "Has Message";
    public Guid SourceNodeTypeId => SessionNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => MessageNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["multiplicity"] = "*",
    };
}
