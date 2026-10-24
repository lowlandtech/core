
namespace LowlandTech.Core.Graph.NodeTypes;

public class TemplateNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Template.Id;

    public Guid Id => NodeTypeId;
    public string Key => "template";
    public string Label => "Template";
    public string? Icon => nameof(Icons.TemplateProcessor);
    public string? BlockComponent => "TemplateBlock";
    public int SortOrder => 10;
    public Type? EntityType => typeof(Template);
    public bool IsListable => true;
    public IReadOnlyList<NavigationHeaderDefinition>? NavigationHeaders =>
    [
        new("Versions", "Versions", "template-version", 0, nameof(Icons.Code)),
        new("Packages", "Packages", "template-package", 1, nameof(Icons.Package))
    ];
}
