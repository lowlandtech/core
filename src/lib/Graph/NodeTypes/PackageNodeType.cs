namespace LowlandTech.Core.Graph.NodeTypes;

/// <summary>
/// Represents a NuGet-style dependency package (with PackageVersion and Imports).
/// Note: EntityType is intentionally null — Package entities are synced to graph nodes
/// via PackageMigrationUseCase and IGraphClient.CreateAsync rather than the auto-sync interceptor.
/// (TemplatePackageNodeType already maps typeof(Package) for the template-bundle use case.)
/// </summary>
public class PackageNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Package.Id;

    public Guid Id => NodeTypeId;
    public string Key => "package";
    public string Label => "Package";
    public string? Icon => nameof(Icons.Package);
    public string? BlockComponent => "PackageBlock";
    public int SortOrder => 13;
    public Type? EntityType => null;
    public bool IsListable => true;
}
