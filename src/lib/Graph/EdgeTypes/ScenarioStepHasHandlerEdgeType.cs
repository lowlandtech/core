namespace LowlandTech.Core.Graph.EdgeTypes;

/// <summary>
/// EdgeType descriptor for the <c>ScenarioStep → Handler</c> create-relationship.
/// verb="create": when a scenario-step node is selected in create mode the command bar
/// auto-binds to this edge and creates a <c>handler</c> child.
/// </summary>
public class ScenarioStepHasHandlerEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.ScenarioStepHasHandler.Id;
    public Guid Id => EdgeTypeId;
    public string  Key              => "scenario-step.has.handler";
    public string  Label            => "Create Handler";
    public Guid    SourceNodeTypeId => ScenarioStepNodeType.NodeTypeId;
    public Guid    TargetNodeTypeId => HandlerNodeType.NodeTypeId;
    public string? ClrType          => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["verb"]        = "create",
        ["tool"]        = "HandlerCreator",
        ["cardinality"] = "many",
        ["direction"]   = "outgoing",
        ["ui.surface"]  = "command-bar",
        ["ui.label"]    = "Create Handler",
    };
}
