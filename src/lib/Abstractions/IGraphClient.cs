namespace LowlandTech.Core.Abstractions;

public interface IGraphClient
{
    Task<T?> GetAsync<T>(Guid id, CancellationToken ct = default) where T : class;
    Task<List<T>> ListAsync<T>(CancellationToken ct = default) where T : class;
    Task<T> CreateAsync<T>(T entity, CancellationToken ct = default) where T : class;
    Task<T?> UpdateAsync<T>(T entity, CancellationToken ct = default) where T : class;
    Task<bool> DeleteAsync<T>(Guid id, CancellationToken ct = default) where T : class;

    Task<object?> GetAsync(Type entityType, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<object>> ListAsync(Type entityType, CancellationToken ct = default);
    Task<object> CreateAsync(Type entityType, object entity, CancellationToken ct = default);
    Task<object?> UpdateAsync(Type entityType, object entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(Type entityType, Guid id, CancellationToken ct = default);
}
