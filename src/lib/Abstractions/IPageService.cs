
namespace LowlandTech.Domain.Abstractions;

public interface IPageService
{
    Task<List<Page>> ListAsync(Guid? workspaceId = null, CancellationToken ct = default);
    Task<Page?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Page> CreateAsync(CreatePageRequest request, CancellationToken ct = default);
    Task<Page?> UpdateAsync(Guid id, UpdatePageRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class CreatePageRequest
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public Guid ContainerId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdatePageRequest
{
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public Guid ContainerId { get; set; }
    public bool IsActive { get; set; }
}
