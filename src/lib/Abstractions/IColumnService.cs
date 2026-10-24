
namespace LowlandTech.Domain.Abstractions;

public interface IColumnService
{
    Task<List<TableColumn>> ListAsync(Guid? tableId = null, CancellationToken ct = default);
    Task<TableColumn?> GetAsync(Guid id, CancellationToken ct = default);
    Task<TableColumn> CreateAsync(TableColumn column, CancellationToken ct = default);
    Task<TableColumn?> UpdateAsync(TableColumn column, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
