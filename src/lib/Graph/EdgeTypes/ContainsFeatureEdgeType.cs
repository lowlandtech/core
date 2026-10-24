
namespace LowlandTech.Core.Graph.EdgeTypes;

public class ContainsFeatureEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.ContainsFeature.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "contains.feature";
    public string Label => "Contains Feature";
    public Guid SourceNodeTypeId => UserStoryNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => FeatureNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>();
}
