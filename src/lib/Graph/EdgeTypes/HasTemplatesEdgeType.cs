
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasTemplatesEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasTemplates.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.templates";
    public string Label => "Has Templates";
    public Guid SourceNodeTypeId => UserContextNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => HeaderNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["required"] = "true",
        ["label"] = "Templates",
        ["icon"] = nameof(Icons.TemplateProcessor),
        ["cascadeDelete"] = "true",
    };
}
