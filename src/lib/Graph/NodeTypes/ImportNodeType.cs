
namespace LowlandTech.Core.Graph.NodeTypes;

public class ImportNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Import.Id;

    public Guid Id => NodeTypeId;
    public string Key => "import";
    public string Label => "Import";
    public string? Icon => nameof(Icons.Download);
    public string? BlockComponent => "ImportBlock";
    public int SortOrder => 14;
    public Type? EntityType => typeof(Import);
    public bool IsListable => true;

    public EntityParentRef ExtractParent(object entity)
    {
        if (entity is not Import import) return EntityParentRef.None;
        return import.PackageId == Guid.Empty
            ? EntityParentRef.None
            : new EntityParentRef("package", import.PackageId);
    }
}
