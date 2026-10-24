
namespace LowlandTech.Domain.Abstractions;

public interface IFlowService
{
	Task<List<Flow>> ListAsync(CancellationToken ct = default);
	Task<Flow?> GetAsync(Guid id, CancellationToken ct = default);
	Task<Flow> CreateAsync(Flow flow, CancellationToken ct = default);
	Task<Flow?> UpdateAsync(Flow flow, CancellationToken ct = default);
	Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}