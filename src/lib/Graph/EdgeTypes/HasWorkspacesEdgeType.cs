
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasWorkspacesEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasWorkspaces.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "contains.workspaces";
    public string Label => "Contains Workspaces";
    public Guid SourceNodeTypeId => UserContextNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => WorkspacesHeaderNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["required"] = "true",
        ["label"] = "Workspaces",
        ["icon"] = nameof(Icons.Workspaces),
        ["cascadeDelete"] = "true",
    };
}
