
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasWorkspaceEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasWorkspace.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "session.has.workspace";
    public string Label => "Has Workspace";
    public Guid SourceNodeTypeId => SessionNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => WorkspaceNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["label"] = "Workspace",
        ["icon"] = nameof(Icons.Workspace),
        ["multiplicity"] = "1",
    };
}
