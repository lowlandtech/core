namespace LowlandTech.Core.Tests.Domain;

public class Plugin1EntityE2ETests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task BeforeSaveTrigger_ShouldSetIsProcessedToTrue()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Plugin1Context>();

        var entity = new Plugin1Entity { Name = "E2E Test Entity" };
        context.Entities.Add(entity);

        // Act
        await context.SaveChangesAsync();

        // Assert
        entity.IsSaving.Should().BeTrue("the BeforeSave trigger should set IsSaving to true");
    }

    [Fact]
    public async Task AfterSaveTrigger_ShouldLogMessage()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Plugin1Context>();

        var entity = new Plugin1Entity { Name = "E2E Test Entity" };
        context.Entities.Add(entity);

        // Act
        await context.SaveChangesAsync();
        
        // Assert
        entity.IsSaved.Should().BeTrue("the BeforeSave trigger should set IsSaving to true");
    }
}