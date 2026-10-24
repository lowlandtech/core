
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasPackagesEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasPackages.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.packages";
    public string Label => "Has Packages";
    public Guid SourceNodeTypeId => UserContextNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => PackagesHeaderNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["required"] = "true",
        ["label"] = "Packages",
        ["icon"] = nameof(Icons.Package),
        ["cascadeDelete"] = "true",
    };
}
