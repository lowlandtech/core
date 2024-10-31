namespace LowlandTech.Core.Tests.Domain;

public class UseCaseExtensionsTests
{
    [Fact]
    public async Task Use_WithDbContextFactory_ShouldSeedData()
    {
        // Arrange: Set up in-memory EF Core DbContext
        var services = new ServiceCollection();
        services.AddDbContextFactory<Plugin1Context>(options => options.UseInMemoryDatabase("Plugin1Schema1"));
        services.AddTransient<Plugin1UseCase>();

        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IDbContextFactory<Plugin1Context>>();

        // Act: Use the UseCaseExtensions to seed data
        await factory.Use<Plugin1UseCase, Plugin1Context>();

        // Assert: Verify that data was seeded
        var context = await factory.CreateDbContextAsync();
        var entityCount = await context.Entities.CountAsync();
        entityCount.Should().Be(1, "one entity should have been seeded by Plugin1UseCase");
    }

    [Fact]
    public async Task Use_WithDbContext_ShouldSeedData()
    {
        // Arrange: Set up in-memory EF Core DbContext
        var options = new DbContextOptionsBuilder<Plugin1Context>()
            .UseInMemoryDatabase("Plugin1Schema2")
            .Options;

        await using var context = new Plugin1Context(options);

        // Act: Use the UseCaseExtensions to seed data directly with DbContext
        await context.Use<Plugin1UseCase, Plugin1Context>();

        // Assert: Verify that data was seeded
        var entityCount = await context.Entities.CountAsync();
        entityCount.Should().Be(1, "one entity should have been seeded by Plugin1UseCase");
    }
}