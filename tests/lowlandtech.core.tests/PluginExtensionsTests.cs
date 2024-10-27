namespace LowlandTech.Core.Tests;

public class PluginExtensionsIntegrationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public void TestPlugins_ShouldBeRegisteredInServiceCollection()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Act
        var Plugin1 = services.GetService<Plugin1.Plugin1>();
        var Plugin2 = services.GetService<Plugin2.Plugin2>();

        // Assert
        Plugin1.Should().NotBeNull("Plugin1 should be registered in the DI container.");
        Plugin2.Should().NotBeNull("Plugin2 should be registered in the DI container.");
    }

    [Fact]
    public void TestPlugins_ShouldBeConfigured()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var plugins = scope.ServiceProvider.GetServices<IPlugin>();

        // Act
        var Plugin1 = plugins.OfType<Plugin1.Plugin1>().FirstOrDefault();
        var Plugin2 = plugins.OfType<Plugin2.Plugin2>().FirstOrDefault();

        // Assert
        Plugin1.Should().NotBeNull("Plugin1 should be registered and configured.");
        Plugin2.Should().NotBeNull("Plugin2 should be registered and configured.");
        Plugin1?.Name.Should().Be("LowlandTech.Plugin1");
        Plugin2?.Name.Should().Be("LowlandTech.Plugin2");
    }

    [Fact]
    public void TestPlugins_ShouldBeAddManually()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var plugins = scope.ServiceProvider.GetServices<IPlugin>();

        // Act
        var plugin3 = plugins.OfType<Plugin3.Plugin3>().FirstOrDefault();

        // Assert
        plugin3.Should().NotBeNull("Plugin3 should be registered and configured.");
        plugin3?.Name.Should().Be("LowlandTech.Plugin3");
    }
}