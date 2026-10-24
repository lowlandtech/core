
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasStatusEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasStatus.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.status";
    public string Label => "Has Status";
    public Guid SourceNodeTypeId => PipelineNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => StepNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["required"] = "true",
        ["mandatory"] = "true",
        ["label"] = "Status",
        ["icon"] = nameof(Icons.Step),
        ["multiplicity"] = "1",
        ["cascadeDelete"] = "true",
    };
}
