using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LowlandTech.Core.Tests.CORE0001.Trimmability.US01.Contract.UseCases;

/// <summary>
/// The Core owned half of UAC05 of CORE0001-US02-SC01: the canonical producer
/// has to stay trimmable, and the entrypoints that the plugin discovery uses
/// at runtime have to carry the attribute that tells the trimmer that they
/// cannot be analyzed.
///
/// The observations are captured here so that every Fact of the scenario
/// asserts one of them against the production assembly.
/// </summary>
public sealed class TrimmableContractUseCase : global::LowlandTech.Core.Tests.Refinement.IUseCase
{
    private readonly List<global::LowlandTech.Core.Tests.Refinement.ResultCase> results = [];

    /// <summary>The named result cases of this scenario.</summary>
    public IReadOnlyList<global::LowlandTech.Core.Tests.Refinement.ResultCase> ResultCases => results;

    /// <summary>The assembly of the producer carries the trimmable marker.</summary>
    public bool? AssemblyIsTrimmable { get; private set; }

    /// <summary>The discovery entrypoints carry the unreferenced code attribute.</summary>
    public bool? DiscoveryEntrypointsAreMarked { get; private set; }

    /// <summary>The diagnostic that the measurement of a named result case recorded.</summary>
    public string Diagnostic(string key)
        => results.FirstOrDefault(result => result.Key == key)?.Diagnostic ?? "no diagnostic was recorded";

    public Task ArrangeAsync() => Task.CompletedTask;

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "This use case asserts that the dynamic plugin-discovery entrypoints carry RequiresUnreferencedCodeAttribute, so it has to observe those members by reflection. The use case never calls the discovery entrypoints themselves.")]
    public Task InvokeAsync()
    {
        var metadata = typeof(NodeTypeRegistry).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == "IsTrimmable" && attribute.Value == "True");
        Record("AssemblyIsTrimmable", metadata is not null,
            metadata is null
                ? "The producer assembly does not carry the trimmable marker."
                : "The producer assembly carries the trimmable marker.");

        var entrypoints = typeof(PluginDiscovery)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => method.Name is nameof(PluginDiscovery.DiscoverApiPlugins) or nameof(PluginDiscovery.DiscoverAppPlugins))
            .ToArray();
        var marked = entrypoints.Length == 2
                     && entrypoints.All(entrypoint =>
                         entrypoint.GetCustomAttribute<RequiresUnreferencedCodeAttribute>() is not null);
        Record("DiscoveryEntrypointsAreMarked", marked,
            $"{entrypoints.Count(entrypoint => entrypoint.GetCustomAttribute<RequiresUnreferencedCodeAttribute>() is not null)}"
            + $" of {entrypoints.Length} discovery entrypoints carry the attribute.");

        return Task.CompletedTask;
    }

    private void Record(string key, bool outcome, string diagnostic)
    {
        results.RemoveAll(result => result.Key == key);
        results.Add(new global::LowlandTech.Core.Tests.Refinement.ResultCase(
            key, outcome ? "Observed" : "NotObserved", diagnostic));
        switch (key)
        {
            case "AssemblyIsTrimmable": AssemblyIsTrimmable = outcome; break;
            case "DiscoveryEntrypointsAreMarked": DiscoveryEntrypointsAreMarked = outcome; break;
        }
    }
}
