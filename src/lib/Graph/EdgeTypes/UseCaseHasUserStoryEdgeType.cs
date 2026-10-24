namespace LowlandTech.Core.Graph.EdgeTypes;

/// <summary>
/// EdgeType descriptor for the <c>UseCase → UserStory</c> create-relationship.
/// verb="create": when a use-case node is selected in create mode the command bar
/// auto-binds to this edge and creates a <c>user-story</c> child.
/// </summary>
public class UseCaseHasUserStoryEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.UseCaseHasUserStory.Id;
    public Guid Id => EdgeTypeId;
    public string  Key              => "use-case.has.user-story";
    public string  Label            => "Create User Story";
    public Guid    SourceNodeTypeId => UseCaseNodeType.NodeTypeId;
    public Guid    TargetNodeTypeId => UserStoryNodeType.NodeTypeId;
    public string? ClrType          => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["verb"]        = "create",
        ["tool"]        = "UserStoryCreator",
        ["cardinality"] = "many",
        ["direction"]   = "outgoing",
        ["ui.surface"]  = "command-bar",
        ["ui.label"]    = "Create User Story",
    };
}
