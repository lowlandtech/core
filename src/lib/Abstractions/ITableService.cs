
namespace LowlandTech.Domain.Abstractions;

public interface ITableService
{
	Task<List<DataTable>> ListAsync(Guid? workspaceId = null, CancellationToken ct = default);
	Task<DataTable> CreateAsync(DataTable table, CancellationToken ct = default);
	Task<DataTable?> GetAsync(Guid id, CancellationToken ct = default);
	Task<DataTable?> UpdateAsync(DataTable table, CancellationToken ct = default);
	Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
