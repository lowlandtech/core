
namespace LowlandTech.Domain.Abstractions;

public interface IWorkflowService
{
    Task<List<Workflow>> ListAsync(Guid? workspaceId = null, CancellationToken ct = default);
    Task<Workflow?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Workflow> CreateAsync(Workflow workflow, CancellationToken ct = default);
    Task<Workflow?> UpdateAsync(Workflow workflow, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
