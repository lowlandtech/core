
namespace LowlandTech.Domain.Abstractions;

public interface IContainerService
{
    Task<List<Container>> ListAsync(CancellationToken ct = default);
    Task<(List<Container> Items, int TotalCount)> ListPagedAsync(int page, int pageSize, string? search, CancellationToken ct = default);
    Task<(List<Container> Items, int TotalCount)> ListPagedAsync(int page, int pageSize, string? search, string? type, CancellationToken ct = default);
    /// <summary>
    /// Drop a palette component onto a canvas. <paramref name="parentItemId"/>
    /// addresses nested grids — when non-null and resolves to an IsGrid
    /// item on the same canvas, the new item lands under it; otherwise
    /// it lands under the canvas's structural root grid.
    /// </summary>
    Task<ContainerItem?> CreateItemFromComponentAsync(Guid containerId, Guid componentId, int? gridColumnStart, int? gridRowStart, int? gridColumnSpan, int? gridRowSpan, Guid? parentItemId = null, CancellationToken ct = default);
    /// <summary>
    /// SPEC2100.US46 step 2 — drop a code-decorated component descriptor
    /// onto a canvas. The frontend resolved the descriptor's element name +
    /// default attribute values from <c>[Properties]</c>/<c>[PropertyGrid]</c>
    /// metadata; the API just persists them. No source-Container required.
    /// </summary>
    Task<ContainerItem?> CreateItemFromDescriptorAsync(Guid containerId, string element, IReadOnlyList<DescriptorAttribute> attributes, int? gridColumnStart, int? gridRowStart, int? gridColumnSpan, int? gridRowSpan, Guid? parentItemId = null, CancellationToken ct = default);
    Task<Container?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Container> CreateAsync(string name, string type, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> UpdateItemPositionAsync(Guid containerId, Guid itemId, int? colStart, int? rowStart, int? colSpan, int? rowSpan, CancellationToken ct = default);
    Task<bool> DeleteItemAsync(Guid containerId, Guid itemId, CancellationToken ct = default);
    Task<bool> UpdateItemAttributeAsync(Guid containerId, Guid itemId, Guid attributeId, string value, CancellationToken ct = default);
    Task<bool> DeleteItemAttributeAsync(Guid containerId, Guid itemId, Guid attributeId, CancellationToken ct = default);
    Task<ContainerAttribute?> UpsertUserAttributeAsync(Guid containerId, Guid itemId, string name, string value, string dataType, CancellationToken ct = default);
    Task<TypeDefinitionDto?> GetTypeDefinitionAsync(Guid containerId, CancellationToken ct = default);
    Task<TypeDefinitionDto?> UpdateTypeDefinitionAsync(Guid containerId, TypeDefinitionDto dto, CancellationToken ct = default);
    Task<bool> UpdateGridTemplateAsync(Guid containerId, string? columns, string? rows, string? areas, string? gap, CancellationToken ct = default);
    Task<bool> UpdateItemAsync(Guid containerId, Guid itemId, string? name, string? element, bool? isGrid, int? columns = null, int? rows = null, CancellationToken ct = default);
    /// <summary>
    /// SPEC2100.US48 — move an item to a different parent grid within the same canvas.
    /// Updates ParentId and grid position. Target parent must be an IsGrid item on
    /// the same canvas; null target moves the item to the canvas root.
    /// </summary>
    Task<bool> MoveItemToParentAsync(Guid containerId, Guid itemId, Guid? parentItemId, int? gridColumnStart, int? gridRowStart, int? gridColumnSpan, int? gridRowSpan, CancellationToken ct = default);
}

/// <summary>
/// Minimum slice of the SPEC2100.US19 type-definition form persisted on
/// Container.Type (kind) + Container.TypeMetadata (JSON-serialised rest).
/// Richer fields (PropDefinitions / ContentAreas / LayoutRegions) defer to
/// follow-up commits.
/// </summary>
public sealed record TypeDefinitionDto(
    string Kind,
    string Description,
    string? Icon,
    IReadOnlyList<string> Tags,
    bool IsPreviewable);

/// <summary>
/// SPEC2100.US46 step 2 — one default attribute carried in the descriptor-
/// drop request. <see cref="Value"/> is the registry's <c>DefaultValue</c>
/// stringified by the frontend; <see cref="DataType"/> mirrors the property's
/// underlying type ("string"/"int"/"bool"/"enum:Foo"/...) so the read path
/// can round-trip it back into the right editor.
/// </summary>
public sealed record DescriptorAttribute(string Name, string Value, string DataType);
