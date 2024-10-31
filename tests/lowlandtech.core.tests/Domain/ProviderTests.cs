namespace LowlandTech.Core.Tests.Domain;

public class ProviderTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task AddProvider_ShouldConfigureDbContextWithInMemoryProvider()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Plugin1Context>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<ProviderOptions>>().Value;

        // Act
        await context.Database.EnsureCreatedAsync();

        // Assert
        context.Database.IsInMemory().Should().BeTrue("the database provider should be in-memory for testing");
        options.Provider.Should().Be(Providers.InMemory, "ProviderOptions should be configured for in-memory provider");
    }
}
