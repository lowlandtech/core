
namespace LowlandTech.Domain.Abstractions;

public interface IWorkspaceService
{
    Task<List<Workspace>> ListAsync(CancellationToken ct = default);
    Task<List<Workspace>> ListAccessibleAsync(string userId, CancellationToken ct = default);
    Task<Workspace?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Workspace> CreateAsync(Workspace workspace, CancellationToken ct = default);
    Task<Workspace?> UpdateAsync(Workspace workspace, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
