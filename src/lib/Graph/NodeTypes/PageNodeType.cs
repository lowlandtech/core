
namespace LowlandTech.Core.Graph.NodeTypes;

public class PageNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Page.Id;

    public Guid Id => NodeTypeId;
    public string Key => "page";
    public string Label => "Page";
    public string? Icon => nameof(Icons.Page);
    public string? BlockComponent => "PageBlock";
    public int SortOrder => 20;
    public Type? EntityType => typeof(Page);
    public bool IsListable => true;
    public IReadOnlyList<NavigationHeaderDefinition>? NavigationHeaders =>
    [
        new("Blocks", "Blocks", "page-item", 0, nameof(Icons.PageBlock))
    ];

    public EntityParentRef ExtractParent(object entity)
        => entity is Page page ? new EntityParentRef("workspace", page.WorkspaceId) : EntityParentRef.None;
}
