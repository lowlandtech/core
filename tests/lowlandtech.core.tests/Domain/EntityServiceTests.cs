namespace LowlandTech.Core.Tests.Domain;

public class EntityServiceTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetEntities_ShouldReturnFilteredEntities()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<EntityService>();

        // Create an EntityQuery to filter users by name "Alice"
        var query = new EntityQuery
        {
            DbContextId = "177f3a1a-e1db-4d85-b66f-fcb18b0005d1",
            DbSetId = "a9b4dfe7-4f29-4c9e-a5e8-cf546edb785b",
            Conditions = [new() 
                { PropertyName = "Name", PropertyValue = "Alice" }
            ]
        };

        // Act
        var users = service.GetEntities<User>(query);

        // Assert
        Assert.NotNull(users);
        Assert.Single(users); // Expect one user
        Assert.Equal("Alice", users[0].Name); // Verify that the returned user is Alice
    }
}