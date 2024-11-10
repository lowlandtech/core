namespace LowlandTech.Core.Tests.Domain;


public class DataContextTests
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options);

    [Fact]
    public void CanResolveTwoNamedDataContextFactories()
    {
        // Set up Lamar container
        var container = new Container(config =>
        {
            // Configure options for each named context
            var options1 = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase("DataContext1")
                .Options;

            var options2 = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase("DataContext2")
                .Options;

            // Register the factories with unique names
            config.For<IDbContextFactory<DataContext>>()
                .Use(ctx => new PooledDbContextFactory<DataContext>(options1))
                .Named("DataContextFactory1");

            config.For<IDbContextFactory<DataContext>>()
                .Use(ctx => new PooledDbContextFactory<DataContext>(options2))
                .Named("DataContextFactory2");
        });

        // Resolve both DbContextFactory instances by name
        var dataContextFactory1 = container.GetInstance<IDbContextFactory<DataContext>>("DataContextFactory1");
        var dataContextFactory2 = container.GetInstance<IDbContextFactory<DataContext>>("DataContextFactory2");

        // Use each factory to create separate DataContext instances
        var dataContext1 = dataContextFactory1.CreateDbContext();
        var dataContext2 = dataContextFactory2.CreateDbContext();

        // Fluent Assertions to verify each factory produces separate contexts
        dataContext1.Should().NotBeNull("because DataContext1 factory should create a valid DbContext");
        dataContext2.Should().NotBeNull("because DataContext2 factory should create a valid DbContext");
        dataContext1.Should().NotBeSameAs(dataContext2, "because each factory should produce a unique DbContext instance");

        // Dispose of contexts if needed
        dataContext1.Dispose();
        dataContext2.Dispose();
    }
}
