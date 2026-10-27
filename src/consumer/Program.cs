using LowlandTech.Core.Graph;
using LowlandTech.Core.Graph.NodeTypes;
using LowlandTech.Core.Plugins;
using LowlandTech.Core.Plugins.InstallModes;
using LowlandTech.Components.Core;

// Graph registry: the packaged registry keeps its declared graph identity.
var registry = new NodeTypeRegistry([new HeaderNodeType()]);
if (registry.FindById(HeaderNodeType.NodeTypeId)?.Key != "header")
    throw new InvalidOperationException("The packaged registry lost its declared graph identity.");

// Plugin descriptor defaults: the packaged descriptor keeps its default install modes.
var descriptor = new PluginDescriptor { Id = "consumer.test", Name = "Consumer", Version = "1.0.0" };
if (!descriptor.InstallModes.SequenceEqual([PluginInstallMode.Routed]))
    throw new InvalidOperationException("The packaged plugin default changed.");

// ComponentsStartupMap: the packaged startup contract keeps its type identity.
var startup = new ComponentsStartupMap([], [], ["backend"]);
if (startup.GetType().Name != "ComponentsStartupMap")
    throw new InvalidOperationException("The packaged startup map contract changed.");

// The report is written without reflection-based serialization, so a trimmed run
// proves the retained contracts do not depend on the reflection-only JSON stack.
Console.WriteLine(
    $"{{\"core\":\"{typeof(NodeTypeRegistry).Assembly.GetName().Name}\"," +
    $"\"componentsCore\":\"{startup.GetType().Assembly.GetName().Name}\"," +
    $"\"startupContract\":\"{startup.GetType().FullName}\"," +
    $"\"registryKey\":\"{registry.All.Single().Key}\"," +
    $"\"installModes\":\"{string.Join(',', descriptor.InstallModes)}\"}}");
