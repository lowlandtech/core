namespace LowlandTech.Core.Graph.NodeTypes;

/// <summary>
/// Typed header descriptor for the <c>Workspaces</c> navigation group under a UserContext.
/// <c>IsHeader = true</c> lets <c>ResolveHeaderEdgeTypes</c> (strategy 3) find this
/// descriptor by label and return the <see cref="WorkspacesHeaderHasWorkspaceEdgeType"/>
/// create-edge so the dashboard command bar shows "Add Workspace" when the header is selected.
/// </summary>
public class WorkspacesHeaderNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.WorkspacesHeader.Id;

    public Guid    Id             => NodeTypeId;
    public string  Key            => "workspaces.header";
    public string  Label          => "Workspaces";
    public string? Icon           => nameof(Icons.Workspaces);
    public string? BlockComponent => null;
    public int     SortOrder      => 0;
    public Type?   EntityType     => null;
    public bool    IsHeader       => true;
}
