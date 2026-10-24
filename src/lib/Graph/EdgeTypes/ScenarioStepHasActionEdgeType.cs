namespace LowlandTech.Core.Graph.EdgeTypes;

/// <summary>
/// EdgeType descriptor for the <c>ScenarioStep → Action</c> create-relationship.
/// verb="create": when a scenario-step node is selected in create mode the command bar
/// auto-binds to this edge and creates an <c>action</c> child.
/// </summary>
public class ScenarioStepHasActionEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.ScenarioStepHasAction.Id;
    public Guid Id => EdgeTypeId;
    public string  Key              => "scenario-step.has.action";
    public string  Label            => "Create Action";
    public Guid    SourceNodeTypeId => ScenarioStepNodeType.NodeTypeId;
    public Guid    TargetNodeTypeId => ActionNodeType.NodeTypeId;
    public string? ClrType          => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["verb"]        = "create",
        ["tool"]        = "ActionCreator",
        ["cardinality"] = "many",
        ["direction"]   = "outgoing",
        ["ui.surface"]  = "command-bar",
        ["ui.label"]    = "Create Action",
    };
}
