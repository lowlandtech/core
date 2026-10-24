
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasToolsEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasTools.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.tools";
    public string Label => "Has Tools";
    public Guid SourceNodeTypeId => UserContextNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => HeaderNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["required"] = "true",
        ["label"] = "Tools",
        ["icon"] = nameof(Icons.Terminal),
        ["cascadeDelete"] = "true",
    };
}
