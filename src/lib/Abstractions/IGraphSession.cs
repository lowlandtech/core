namespace LowlandTech.Core.Abstractions;

public interface IGraphSession
{
    IQueryable<T> Query<T>() where T : class;

    Task<T?> GetAsync<T>(
        Guid id,
        Func<IQueryable<T>, IQueryable<T>>? query = null,
        CancellationToken ct = default) where T : class;

    Task<List<T>> ListAsync<T>(
        Func<IQueryable<T>, IQueryable<T>>? query = null,
        CancellationToken ct = default) where T : class;

    Task<T> AddAsync<T>(T entity, CancellationToken ct = default) where T : class;
    Task<T?> UpdateAsync<T>(T entity, CancellationToken ct = default) where T : class;
    Task<bool> DeleteAsync<T>(Guid id, CancellationToken ct = default) where T : class;

    Task<object?> GetAsync(Type entityType, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<object>> ListAsync(Type entityType, CancellationToken ct = default);
    Task<object> AddAsync(Type entityType, object entity, CancellationToken ct = default);
    Task<object?> UpdateAsync(Type entityType, object entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(Type entityType, Guid id, CancellationToken ct = default);
}
