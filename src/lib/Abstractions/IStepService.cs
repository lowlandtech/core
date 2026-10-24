
namespace LowlandTech.Domain.Abstractions;

public interface IStepService
{
    Task<List<Step>> ListAsync(Guid? flowId = null, CancellationToken ct = default);
    Task<Step?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Step> CreateAsync(Step step, CancellationToken ct = default);
    Task<Step?> UpdateAsync(Step step, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
