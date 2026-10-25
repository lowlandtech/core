using System.Text.Json;
using LowlandTech.Core.Graph;
using LowlandTech.Core.Graph.NodeTypes;
using LowlandTech.Core.Plugins;
using LowlandTech.Core.Plugins.InstallModes;
using LowlandTech.Components.Core;

var registry = new NodeTypeRegistry([new HeaderNodeType()]);
if (registry.FindById(HeaderNodeType.NodeTypeId)?.Key != "header")
    throw new InvalidOperationException("The packaged registry lost its declared graph identity.");
var descriptor = new PluginDescriptor { Id = "consumer.test", Name = "Consumer", Version = "1.0.0" };
if (!descriptor.InstallModes.SequenceEqual([PluginInstallMode.Routed]))
    throw new InvalidOperationException("The packaged plugin default changed.");
var startup = new ComponentsStartupMap([], [], ["backend"]);
Console.WriteLine(JsonSerializer.Serialize(new {
    core = typeof(NodeTypeRegistry).Assembly.GetName().Name,
    componentsCore = startup.GetType().Assembly.GetName().Name,
    startupContract = startup.GetType().FullName,
    registryKey = registry.All.Single().Key
}));
