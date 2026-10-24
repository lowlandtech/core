namespace LowlandTech.Core.Graph.EdgeTypes;

/// <summary>
/// EdgeType descriptor for the <c>Scenario → ScenarioStep</c> create-relationship.
/// verb="create": when a scenario node is selected in create mode the command bar
/// auto-binds to this edge and creates a <c>scenario-step</c> child.
/// </summary>
public class ScenarioHasScenarioStepEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.ScenarioHasScenarioStep.Id;
    public Guid Id => EdgeTypeId;
    public string  Key              => "scenario.has.scenario-step";
    public string  Label            => "Create Scenario Step";
    public Guid    SourceNodeTypeId => ScenarioNodeType.NodeTypeId;
    public Guid    TargetNodeTypeId => ScenarioStepNodeType.NodeTypeId;
    public string? ClrType          => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["verb"]        = "create",
        ["tool"]        = "ScenarioStepCreator",
        ["cardinality"] = "many",
        ["direction"]   = "outgoing",
        ["ui.surface"]  = "command-bar",
        ["ui.label"]    = "Create Scenario Step",
    };
}
