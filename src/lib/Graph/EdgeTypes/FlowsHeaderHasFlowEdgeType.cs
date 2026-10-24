namespace LowlandTech.Core.Graph.EdgeTypes;

/// <summary>
/// EdgeType descriptor for the <c>FlowsHeader → Flow</c> create-relationship.
/// verb="create": when the Flows header is selected in the dashboard tree the
/// session command bar auto-binds to this edge and presents "Add Flow" as the
/// primary create intent.  On submit, <see cref="DashboardPage.SubmitCreateNode"/>
/// creates a <c>flow</c> node and a <c>FlowsHeader-has-flow</c> edge, then opens
/// the resulting node in a <see cref="FlowNodeType.BlockComponent">FlowBlock</see>.
/// </summary>
public class FlowsHeaderHasFlowEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.FlowsHeaderHasFlow.Id;
    public Guid Id => EdgeTypeId;
    public string  Key              => "flow.header.has.flow";
    public string  Label            => "Create Flow";
    public Guid    SourceNodeTypeId => FlowsHeaderNodeType.NodeTypeId;
    public Guid    TargetNodeTypeId => FlowNodeType.NodeTypeId;
    public string? ClrType          => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["verb"]        = "create",
        ["tool"]        = "FlowCreator",
        ["cardinality"] = "many",
        ["direction"]   = "outgoing",
        ["ui.surface"]  = "command-bar",
        ["ui.label"]    = "Create Flow",
    };
}
