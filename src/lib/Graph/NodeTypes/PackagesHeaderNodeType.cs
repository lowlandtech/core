namespace LowlandTech.Core.Graph.NodeTypes;

/// <summary>
/// NodeType descriptor for the <c>Packages</c> header in the user-context tree.
/// Headers group entities under a domain label without backing any entity row.
/// <c>IsHeader = true</c> lets the dashboard registry resolve the correct
/// <see cref="LowlandTech.Core.Graph.EdgeTypes.PackagesHeaderHasPackageEdgeType"/>
/// (verb="create") so the session command bar shows "Add Package" when this header
/// is selected.
/// </summary>
public class PackagesHeaderNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.PackagesHeader.Id;

    public Guid    Id             => NodeTypeId;
    public string  Key            => "package.header";
    public string  Label          => "Packages";
    public string? Icon           => nameof(Icons.Package);
    public string? BlockComponent => null;
    public int     SortOrder      => 0;
    public Type?   EntityType     => null;
    public bool    IsHeader       => true;
}
