
namespace LowlandTech.Core.Graph.NodeTypes;

public class ToolNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Tool.Id;

    public Guid Id => NodeTypeId;
    public string Key => "tool";
    public string Label => "Tool";
    public string? Icon => nameof(Icons.Terminal);
    public string? BlockComponent => "ToolBlock";
    public int SortOrder => 15;
    public Type? EntityType => typeof(Tool);
    public bool IsListable => true;
    public IReadOnlyList<NavigationHeaderDefinition>? NavigationHeaders =>
    [
        new("Commands", "Commands", "tool-command", 0, nameof(Icons.Code))
    ];
}
