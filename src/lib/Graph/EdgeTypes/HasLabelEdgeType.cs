
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasLabelEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasLabel.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.label";
    public string Label => "Has Label";
    public Guid SourceNodeTypeId => SessionNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => LabelNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["label"] = "Label",
        ["icon"] = nameof(Icons.Hash),
        ["multiplicity"] = "*",
        ["cascadeDelete"] = "true",
    };
}
