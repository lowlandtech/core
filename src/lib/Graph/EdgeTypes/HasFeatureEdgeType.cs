
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasFeatureEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasFeature.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.feature";
    public string Label => "Has Feature";
    public Guid SourceNodeTypeId => HeaderNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => FeatureNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>();
}
