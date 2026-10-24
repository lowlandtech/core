
namespace LowlandTech.Core.Graph.NodeTypes;

public class WorkspaceNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Workspace.Id;

    public Guid Id => NodeTypeId;
    public string Key => "workspace";
    public string Label => "Workspace";
    public string? Icon => nameof(Icons.Workspace);
    public string? BlockComponent => "WorkspaceBlock";
    public int SortOrder => 3;
    public Type? EntityType => typeof(Workspace);
    public bool IsListable => true;
    public IReadOnlyList<NavigationHeaderDefinition>? NavigationHeaders =>
    [
        new("Tables", "Tables", "table", 0, nameof(Icons.Tables)),
        new("Pages", "Pages", "page", 1, nameof(Icons.Pages)),
        new("Workflows", "Workflows", "workflow", 2, nameof(Icons.Workflows)),
        new("Branches", "Branches", "branch", 3, nameof(Icons.GitBranch)),
        new("Specifications", "Specifications", "feature", 4, nameof(Icons.File),
            ParentToHeaderEdgeKey: "has.features", HeaderToChildEdgeKey: "has.feature"),
    ];
}
