namespace LowlandTech.Core.Domain;

/// <summary>
/// Extension methods for use cases.
/// </summary>
public static class UseCaseExtensions
{
    #region Method: "Use"
    /// <summary>
    /// Uses the specified use case.
    /// </summary>
    /// <param name="factory">The DbFactory.</param>
    /// <typeparam name="T">The use-case.</typeparam>
    /// <typeparam name="TContext">The DbContext.</typeparam>
    public static async Task Use<T, TContext>(this IDbContextFactory<TContext> factory)
        where T : class, IUseCase
        where TContext : DbContext
    {
        var context = await factory.CreateDbContextAsync();
        var instance = (T)Activator.CreateInstance(typeof(T), [context])!;
        await instance.SeedAsync();
    }

    /// <summary>
    /// Uses the specified use case.
    /// </summary>
    /// <param name="context">The DbContext</param>
    /// <typeparam name="T">The use-case.</typeparam>
    /// <typeparam name="TContext">The DbContext.</typeparam>
    public static async Task Use<T, TContext>(this TContext context)
        where T : class, IUseCase
        where TContext : DbContext
    {
        var instance = (T)Activator.CreateInstance(typeof(T), [context])!;
        await instance.SeedAsync();
    }
    #endregion
}
