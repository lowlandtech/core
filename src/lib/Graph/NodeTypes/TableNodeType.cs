
namespace LowlandTech.Core.Graph.NodeTypes;

public class TableNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Table.Id;

    public Guid Id => NodeTypeId;
    public string Key => "table";
    public string Label => "Table";
    public string? Icon => nameof(Icons.Table);
    public string? BlockComponent => "TableBlock";
    public int SortOrder => 4;
    public Type? EntityType => typeof(DataTable);
    public bool IsListable => true;
    public IReadOnlyList<NavigationHeaderDefinition>? NavigationHeaders =>
    [
        new(
            NavigationProperty: "Columns",
            HeaderLabel: "Columns",
            ChildNodeTypeKey: "column",
            SortOrder: 0,
            HeaderIcon: nameof(Icons.Columns))
    ];

    public EntityParentRef ExtractParent(object entity)
        => entity is DataTable table ? new EntityParentRef("workspace", table.WorkspaceId) : EntityParentRef.None;
}
