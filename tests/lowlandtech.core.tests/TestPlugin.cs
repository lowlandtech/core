namespace LowlandTech.Core.Tests;

// Mock class for IPlugin
[PluginId("954f8375-965b-47e7-83bb-a9752827267f")]
public class TestPlugin : Plugin
{
    public TestPlugin()
    {
        Id = new("954f8375-965b-47e7-83bb-a9752827267f");
        Name = "Plugin3";
    }

    public override void Install(ServiceRegistry services)
    {
        // Mocked installation logic
    }

    public override void Configure(WebApplication app)
    {
        // Mocked configuration logic
    }
}
