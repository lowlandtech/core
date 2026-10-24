namespace LowlandTech.Core.Plugins;

/// <summary>
/// Resolves any plugin's DbContext by ID.
/// Every plugin contributes via services.AddScoped&lt;IDbFactoryContext, MyDbFactoryContext&gt;().
/// Any plugin can consume this service to query across all registered data sources.
/// </summary>
public class DbContextService(IEnumerable<IDbFactoryContext> contexts)
{
    public IDbFactoryContext Get(Guid id) =>
        contexts.FirstOrDefault(c => c.Id == id)
        ?? throw new InvalidOperationException($"No DbContext registered for id {id}");

    public IQueryable<object> Query(Guid contextId, string entityType, string criteria) =>
        Get(contextId).Query(entityType, criteria);
}
