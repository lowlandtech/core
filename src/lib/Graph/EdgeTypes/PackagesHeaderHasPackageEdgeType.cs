namespace LowlandTech.Core.Graph.EdgeTypes;

/// <summary>
/// EdgeType descriptor for the <c>PackagesHeader → Package</c> create-relationship.
/// verb="create": when the Packages header is selected in the dashboard tree the
/// session command bar auto-binds to this edge and presents "Add Package" as the
/// primary create intent.  On submit, <see cref="DashboardPage.SubmitCreateNode"/>
/// creates a <c>package</c> node and a <c>PackagesHeader-has-package</c> edge,
/// then opens the resulting node in a <see cref="PackageNodeType.BlockComponent">PackageBlock</see>.
/// </summary>
public class PackagesHeaderHasPackageEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.PackagesHeaderHasPackage.Id;
    public Guid Id => EdgeTypeId;
    public string  Key              => "package.header.has.package";
    public string  Label            => "Create Package";
    public Guid    SourceNodeTypeId => PackagesHeaderNodeType.NodeTypeId;
    public Guid    TargetNodeTypeId => PackageNodeType.NodeTypeId;
    public string? ClrType          => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["verb"]        = "create",
        ["tool"]        = "PackageCreator",
        ["cardinality"] = "many",
        ["direction"]   = "outgoing",
        ["ui.surface"]  = "command-bar",
        ["ui.label"]    = "Create Package",
    };
}
