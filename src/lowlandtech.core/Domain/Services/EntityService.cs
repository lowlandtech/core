namespace LowlandTech.Core.Domain.Services;

/// <summary>
/// Represents an entity service.
/// </summary>
/// <param name="container">The lamar IOC container</param>
public class EntityService(IContainer container)
{
    /// <summary>
    /// Gets the entities info for a context.
    /// </summary>
    /// <typeparam name="TContext">The DbContext</typeparam>
    /// <returns>A list of entity infos</returns>
    public List<EntityInfo> GetEntitiesInfo<TContext>() where TContext : DbContext
    {
        var entities = new List<EntityInfo>();

        // Get DbContext attribute
        var dbContextIdAttribute = typeof(TContext).GetCustomAttribute<DbContextIdAttribute>();

        // Get all DbSet properties with DbSetId attribute
        var dbSetProperties = typeof(TContext)
            .GetProperties()
            .Where(prop => prop.PropertyType.IsGenericType)
            .Where(prop => prop.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        foreach (var prop in dbSetProperties)
        {
            var dbSetIdAttribute = prop.GetCustomAttribute<DbSetIdAttribute>();
            if(dbSetIdAttribute is null) continue;

            var dbSetId = dbSetIdAttribute!.Id;
            var dbSetName = prop.Name;

            entities.Add(new EntityInfo
            {
                Name = dbSetName,
                Id = dbSetId,
                ContextId = dbContextIdAttribute!.Id
            });
        }

        return entities;
    }

    /// <summary>
    /// Gets the entities for a query.
    /// </summary>
    /// <param name="query">The entity query</param>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <typeparam name="TContext"></typeparam>
    /// <returns>List of entities</returns>
    /// <exception cref="ArgumentException"></exception>
    public List<TEntity> GetEntities<TEntity>(EntityQuery query) where TEntity : class
    {
        // Resolve the DbContextFactory from the container using DbContextId
        var factory = container.GetInstance<IDbContextFactory<DbContext>>(query.DbContextId);
        var context = factory.CreateDbContext();

        // Get DbSet property by entity name
        var dbSetProperty = context.GetType().GetProperties().FirstOrDefault(p => p.GetCustomAttribute<DbSetIdAttribute>()?.Id == query.DbSetId);
        if (dbSetProperty == null)
        {
            throw new ArgumentException($"Entity with ID {query.DbSetId} not found in context.");
        }

        var dbSet = dbSetProperty.GetValue(context) as IQueryable<TEntity>;
        if (dbSet == null)
        {
            throw new ArgumentException($"DbSet for entity {query.DbSetId} could not be resolved.");
        }

        // Build a Dynamic LINQ query using the conditions
        var queryable = dbSet.AsQueryable();
        foreach (var condition in query.Conditions)
        {
            queryable = queryable.Where($"{condition.PropertyName} == @0", condition.PropertyValue);
        }

        return queryable.ToList();
    }
}
