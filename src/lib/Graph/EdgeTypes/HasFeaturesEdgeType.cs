
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasFeaturesEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasFeatures.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.features";
    public string Label => "Has Features";
    public Guid SourceNodeTypeId => WorkspaceNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => HeaderNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["required"] = "false",
        ["label"] = "Specifications",
        ["icon"] = nameof(Icons.File),
        ["cascadeDelete"] = "true",
    };
}
