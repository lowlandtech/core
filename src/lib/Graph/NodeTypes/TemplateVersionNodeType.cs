
namespace LowlandTech.Core.Graph.NodeTypes;

public class TemplateVersionNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.TemplateVersion.Id;

    public Guid Id => NodeTypeId;
    public string Key => "template-version";
    public string Label => "Version";
    public string? Icon => nameof(Icons.Code);
    public string? BlockComponent => "TemplateVersionBlock";
    public int SortOrder => 11;
    public Type? EntityType => typeof(TemplateVersion);
    public bool IsListable => true;

    public EntityParentRef ExtractParent(object entity)
        => entity is TemplateVersion version ? new EntityParentRef("template", version.TemplateId) : EntityParentRef.None;
}
