namespace LowlandTech.Core.Tests.CORE0001.CoreContracts.US01.Registry.UseCases;

/// <summary>
/// The contracts of the canonical producer that a consumer of the package
/// depends on: the registry keeps the identity and the order of the
/// descriptors it is given, the entity projection keeps the static identity of
/// a descriptor, a duplicate descriptor is refused before the lookup, and the
/// default install mode of a plugin descriptor stays routed.
///
/// The observations are captured here so that every Fact of the scenario
/// asserts one of them against the production types.
/// </summary>
public sealed class CoreContractUseCase : global::LowlandTech.Core.Tests.Refinement.IUseCase
{
    private readonly List<global::LowlandTech.Core.Tests.Refinement.ResultCase> results = [];

    /// <summary>The named result cases of this scenario.</summary>
    public IReadOnlyList<global::LowlandTech.Core.Tests.Refinement.ResultCase> ResultCases => results;

    /// <summary>The registry keeps the order of the descriptors it is given.</summary>
    public bool? RegistryPreservesOrder { get; private set; }

    /// <summary>The registry finds the descriptor of an entity type.</summary>
    public bool? RegistryFindsByEntityType { get; private set; }

    /// <summary>The entity projection keeps the static identity of a descriptor.</summary>
    public bool? EntityProjectionIsStatic { get; private set; }

    /// <summary>A duplicate descriptor is refused before the lookup.</summary>
    public bool? DuplicateDescriptorsAreRefused { get; private set; }

    /// <summary>The default install mode of a plugin descriptor stays routed.</summary>
    public bool? PluginDefaultIsRouted { get; private set; }

    /// <summary>The diagnostic that the measurement of a named result case recorded.</summary>
    public string Diagnostic(string key)
        => results.FirstOrDefault(result => result.Key == key)?.Diagnostic ?? "no diagnostic was recorded";

    public Task ArrangeAsync() => Task.CompletedTask;

    public Task InvokeAsync()
    {
        var bound = new BoundNodeDescriptor();
        var registry = new NodeTypeRegistry([bound, new HeaderNodeType()]);

        var keys = registry.All.Select(entry => entry.Key).ToArray();
        Record("RegistryPreservesOrder",
            keys.SequenceEqual(["header", "extraction.bound"]),
            $"The registry published the keys {string.Join(", ", keys)}.");

        var found = registry.FindByEntityType(typeof(Node));
        Record("RegistryFindsByEntityType",
            found is not null && found.Id == bound.Id,
            found is null
                ? "The registry did not find the descriptor of the entity type."
                : $"The registry found the descriptor {found.Id}.");

        var entity = NodeTypeRegistry.ToEntity(new NodeTypeRegistry([new HeaderNodeType()]).All.Single());
        Record("EntityProjectionIsStatic",
            entity.Id == HeaderNodeType.NodeTypeId && entity.IsStatic,
            $"The entity projection carries the id {entity.Id} and the static flag {entity.IsStatic}.");

        string refusal;
        try
        {
            _ = new NodeTypeRegistry([new BoundNodeDescriptor(), new BoundNodeDescriptor()]);
            refusal = "the registry accepted two descriptors of the same kind";
        }
        catch (InvalidOperationException exception)
        {
            refusal = exception.Message;
        }
        Record("DuplicateDescriptorsAreRefused",
            refusal.Contains("Multiple INodeTypeDescriptor", StringComparison.Ordinal),
            $"The registry answered the duplicate descriptors with: {refusal}");

        var modes = new PluginDescriptor { Id = "extraction.test", Name = "Test", Version = "1.0.0" }.InstallModes;
        Record("PluginDefaultIsRouted",
            modes.SequenceEqual([PluginInstallMode.Routed]),
            $"The plugin descriptor declares the install modes {string.Join(", ", modes)}.");

        return Task.CompletedTask;
    }

    private void Record(string key, bool outcome, string diagnostic)
    {
        results.RemoveAll(result => result.Key == key);
        results.Add(new global::LowlandTech.Core.Tests.Refinement.ResultCase(
            key, outcome ? "Observed" : "NotObserved", diagnostic));
        switch (key)
        {
            case "RegistryPreservesOrder": RegistryPreservesOrder = outcome; break;
            case "RegistryFindsByEntityType": RegistryFindsByEntityType = outcome; break;
            case "EntityProjectionIsStatic": EntityProjectionIsStatic = outcome; break;
            case "DuplicateDescriptorsAreRefused": DuplicateDescriptorsAreRefused = outcome; break;
            case "PluginDefaultIsRouted": PluginDefaultIsRouted = outcome; break;
        }
    }
}
