namespace LowlandTech.Core.Tests;

public class PluginApiIntegrationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetPlugins_ShouldReturnListOfPlugins()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/plugins");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var plugins = await response.Content.ReadFromJsonAsync<List<PluginModel>>();

        plugins.Should().NotBeNull();
        plugins.Count.Should().Be(3);
    }
}

public class PluginModel
{
    public Guid Id { get; }
    public string? Name { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; }
    public string? Company { get; }
    public string? Copyright { get; }
    public string? Url { get; }
    public string? Version { get; }
    public string? Authors { get; }
}