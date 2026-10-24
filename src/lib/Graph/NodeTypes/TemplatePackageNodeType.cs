
namespace LowlandTech.Core.Graph.NodeTypes;

public class TemplatePackageNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.TemplatePackage.Id;

    public Guid Id => NodeTypeId;
    public string Key => "template-package";
    public string Label => "Package";
    public string? Icon => nameof(Icons.Package);
    public string? BlockComponent => "TemplatePackageBlock";
    public int SortOrder => 12;
    public Type? EntityType => typeof(Package);
    public bool IsListable => true;

    public EntityParentRef ExtractParent(object entity)
    {
        if (entity is not Package package) return EntityParentRef.None;
        var template = package.Templates.FirstOrDefault();
        return template is null || template.Id == Guid.Empty
            ? EntityParentRef.None
            : new EntityParentRef("template", template.Id);
    }
}
