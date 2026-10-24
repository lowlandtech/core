
namespace LowlandTech.Core.Graph.NodeTypes;

public class BranchNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Branch.Id;

    public Guid Id => NodeTypeId;
    public string Key => "branch";
    public string Label => "Branch";
    public string? Icon => nameof(Icons.GitBranch);
    public string? BlockComponent => "GitRepoBlock";
    public int SortOrder => 6;
    public Type? EntityType => typeof(Branch);
    public bool IsListable => true;

    public EntityParentRef ExtractParent(object entity)
        => entity is Branch branch ? new EntityParentRef("workspace", branch.WorkspaceId) : EntityParentRef.None;
}
