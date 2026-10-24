namespace LowlandTech.Core.Plugins;

/// <summary>
/// A queryable data source backed by a plugin's DbContext.
/// Every plugin registers one implementation; any plugin can resolve the full collection via DbContextService.
/// </summary>
public interface IDbFactoryContext
{
    Guid Id { get; }
    string Name { get; }
    IQueryable<object> Query(string entityType, string criteria);
}
