namespace LowlandTech.Core.Graph.EdgeTypes;

/// <summary>
/// EdgeType descriptor for the <c>UserStory → Scenario</c> create-relationship.
/// verb="create": when a user-story node is selected in create mode the command bar
/// auto-binds to this edge and creates a <c>scenario</c> child.
/// </summary>
public class UserStoryHasScenarioEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.UserStoryHasScenario.Id;
    public Guid Id => EdgeTypeId;
    public string  Key              => "user-story.has.scenario";
    public string  Label            => "Create Scenario";
    public Guid    SourceNodeTypeId => UserStoryNodeType.NodeTypeId;
    public Guid    TargetNodeTypeId => ScenarioNodeType.NodeTypeId;
    public string? ClrType          => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["verb"]        = "create",
        ["tool"]        = "ScenarioCreator",
        ["cardinality"] = "many",
        ["direction"]   = "outgoing",
        ["ui.surface"]  = "command-bar",
        ["ui.label"]    = "Create Scenario",
    };
}
