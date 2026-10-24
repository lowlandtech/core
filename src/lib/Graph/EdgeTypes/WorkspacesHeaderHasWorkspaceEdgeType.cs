namespace LowlandTech.Core.Graph.EdgeTypes;

/// <summary>
/// EdgeType descriptor for the <c>WorkspacesHeader → Workspace</c> create-relationship.
/// verb="create": when the Workspaces header is selected in the dashboard tree the
/// session command bar auto-binds to this edge and presents "Add Workspace" as the
/// primary create intent.
/// </summary>
public class WorkspacesHeaderHasWorkspaceEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.WorkspacesHeaderHasWorkspace.Id;
    public Guid Id => EdgeTypeId;
    public string  Key              => "has.workspace";
    public string  Label            => "Create Workspace";
    public Guid    SourceNodeTypeId => WorkspacesHeaderNodeType.NodeTypeId;
    public Guid    TargetNodeTypeId => WorkspaceNodeType.NodeTypeId;
    public string? ClrType          => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["verb"]        = "create",
        ["tool"]        = "WorkspaceCreator",
        ["cardinality"] = "many",
        ["direction"]   = "outgoing",
        ["ui.surface"]  = "command-bar",
        ["ui.label"]    = "Create Workspace",
    };
}
