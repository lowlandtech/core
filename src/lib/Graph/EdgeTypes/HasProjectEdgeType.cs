
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasProjectEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasProject.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.project";
    public string Label => "Has Project";
    public Guid SourceNodeTypeId => SessionNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => ProjectNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["label"] = "Project",
        ["icon"] = nameof(Icons.Briefcase),
        ["multiplicity"] = "1",
        ["cascadeDelete"] = "true",
    };
}
