
namespace LowlandTech.Core.Graph.NodeTypes;

public class PageItemNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.PageItem.Id;

    public Guid Id => NodeTypeId;
    public string Key => "page-item";
    public string Label => "Page Block";
    public string? Icon => nameof(Icons.PageBlock);
    public string? BlockComponent => "PageItemBlock";
    public int SortOrder => 51;
    public Type? EntityType => typeof(PageItem);
    public bool IsListable => true;
    public IReadOnlyList<NavigationHeaderDefinition>? NavigationHeaders => [];
}
