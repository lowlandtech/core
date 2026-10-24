
namespace LowlandTech.Domain.Abstractions;

public interface IBranchService
{
    Task<List<Branch>> ListAsync(Guid? workspaceId = null, CancellationToken ct = default);
    Task<Branch?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Branch> CreateAsync(Branch branch, CancellationToken ct = default);
    Task<Branch?> UpdateAsync(Branch branch, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
