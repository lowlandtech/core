namespace LowlandTech.Core.Graph.EdgeTypes;

/// <summary>
/// EdgeType descriptor for the <c>ScenarioStep → Event</c> create-relationship.
/// verb="create": when a scenario-step node is selected in create mode the command bar
/// auto-binds to this edge and creates an <c>event</c> child.
/// </summary>
public class ScenarioStepHasEventEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.ScenarioStepHasEvent.Id;
    public Guid Id => EdgeTypeId;
    public string  Key              => "scenario-step.has.event";
    public string  Label            => "Create Event";
    public Guid    SourceNodeTypeId => ScenarioStepNodeType.NodeTypeId;
    public Guid    TargetNodeTypeId => EventNodeType.NodeTypeId;
    public string? ClrType          => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["verb"]        = "create",
        ["tool"]        = "EventCreator",
        ["cardinality"] = "many",
        ["direction"]   = "outgoing",
        ["ui.surface"]  = "command-bar",
        ["ui.label"]    = "Create Event",
    };
}
