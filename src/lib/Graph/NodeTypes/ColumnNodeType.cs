
namespace LowlandTech.Core.Graph.NodeTypes;

public class ColumnNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Column.Id;

    public Guid Id => NodeTypeId;
    public string Key => "column";
    public string Label => "Column";
    public string? Icon => nameof(Icons.Column);
    public string? BlockComponent => "ColumnBlock";
    public int SortOrder => 7;
    public Type? EntityType => typeof(TableColumn);
    public bool IsListable => true;

    public EntityParentRef ExtractParent(object entity)
        => entity is TableColumn column ? new EntityParentRef("table", column.TableId) : EntityParentRef.None;
}
