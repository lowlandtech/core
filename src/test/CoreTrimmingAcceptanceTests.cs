using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LowlandTech.Core.Tests;

public sealed class CoreTrimmingAcceptanceTests
{
    [Fact]
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "This Fact asserts that the dynamic plugin-discovery entrypoints carry RequiresUnreferencedCodeAttribute, so it has to observe those members by reflection. The Fact never calls the discovery entrypoints themselves.")]
    public void UAC05_TrimmablePackageMarksDynamicDiscoveryUnreferencedCode()
    {
        var metadata = typeof(NodeTypeRegistry).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == "IsTrimmable" && attribute.Value == "True");
        metadata.ShouldNotBeNull();

        var entrypoints = typeof(PluginDiscovery).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => method.Name is nameof(PluginDiscovery.DiscoverApiPlugins) or nameof(PluginDiscovery.DiscoverAppPlugins))
            .ToArray();
        entrypoints.Length.ShouldBe(2);
        foreach (var entrypoint in entrypoints)
            entrypoint.GetCustomAttribute<RequiresUnreferencedCodeAttribute>().ShouldNotBeNull();
    }
}
