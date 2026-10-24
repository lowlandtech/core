
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasFlowsEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasFlows.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.flows";
    public string Label => "Has Flows";
    public Guid SourceNodeTypeId => UserContextNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => FlowsHeaderNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["required"] = "true",
        ["label"] = "Flows",
        ["icon"] = nameof(Icons.Workflows),
        ["cascadeDelete"] = "true",
    };
}
