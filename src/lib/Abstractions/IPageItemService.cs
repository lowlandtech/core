
namespace LowlandTech.Domain.Abstractions;

public interface IPageItemService
{
    Task<PageItem?> GetAsync(Guid id, CancellationToken ct = default);
    Task<List<PageItem>> GetByPageAsync(Guid pageId, CancellationToken ct = default);
    Task<PageItem> CreateAsync(CreatePageItemRequest request, CancellationToken ct = default);
    Task<PageItem?> UpdateAsync(Guid id, UpdatePageItemRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<PageItem?> MoveAsync(Guid id, Guid? newParentId, CancellationToken ct = default);
}

public class CreatePageItemRequest
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Element { get; set; }
    public string ElementType { get; set; } = "LayoutContainer";
    public Guid? ContainerItemId { get; set; }
    public Guid? ParentId { get; set; }
    public bool? IsGrid { get; set; }
    public int? GridColumnStart { get; set; }
    public int? GridColumnEnd { get; set; }
    public int? GridRowStart { get; set; }
    public int? GridRowEnd { get; set; }
}

public class UpdatePageItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Element { get; set; }
    public string ElementType { get; set; } = "LayoutContainer";
    public bool? IsGrid { get; set; }
    public int? GridColumnStart { get; set; }
    public int? GridColumnEnd { get; set; }
    public int? GridRowStart { get; set; }
    public int? GridRowEnd { get; set; }
    public bool IsActive { get; set; }
}
