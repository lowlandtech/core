using System.Diagnostics.CodeAnalysis;

namespace LowlandTech.Core.Plugins;

/// <summary>
/// Base class for plugin data source adapters.
/// Auto-registers every DbSet&lt;T&gt; property on TContext — no switch statement needed in subclasses.
/// </summary>
public abstract class DbFactoryContext<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TContext>(IDbContextFactory<TContext> factory)
    : IDbFactoryContext, IDisposable
    where TContext : DbContext
{
    private TContext? _db;

    // Kept alive so callers can enumerate the returned IQueryable<object> before scope ends
    protected TContext Db => _db ??= factory.CreateDbContext();

    public abstract Guid Id { get; }
    public abstract string Name { get; }

    private Dictionary<string, Func<TContext, IQueryable<object>>>? _registry;
    private Dictionary<string, Func<TContext, IQueryable<object>>> Registry => _registry ??= BuildRegistry();

    private static Dictionary<string, Func<TContext, IQueryable<object>>> BuildRegistry()
    {
        var registry = new Dictionary<string, Func<TContext, IQueryable<object>>>();

        foreach (var prop in typeof(TContext).GetProperties()
            .Where(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)))
        {
            var entityName = prop.PropertyType.GetGenericArguments()[0].Name;
            var captured   = prop;
            registry[entityName] = db => (IQueryable<object>)captured.GetValue(db)!;
        }

        return registry;
    }

    public IQueryable<object> Query(string entityType, string criteria)
    {
        if (!Registry.TryGetValue(entityType, out var accessor))
            throw new InvalidOperationException($"Entity '{entityType}' not found in context '{Name}'");

        return accessor(Db);
    }

    public void Dispose()
    {
        _db?.Dispose();
        _db = null;
        GC.SuppressFinalize(this);
    }
}
