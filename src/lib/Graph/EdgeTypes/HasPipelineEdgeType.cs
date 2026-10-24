
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasPipelineEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasPipeline.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.pipeline";
    public string Label => "Has Pipeline";
    public Guid SourceNodeTypeId => SessionNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => PipelineNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["label"] = "Pipeline",
        ["icon"] = nameof(Icons.Pipeline),
        ["multiplicity"] = "1",
        ["cascadeDelete"] = "true",
    };
}
