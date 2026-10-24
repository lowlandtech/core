namespace LowlandTech.Core.Plugins;

/// <summary>
/// Operations for enabling, disabling, and remapping features at runtime without restart.
/// SPEC2050 US03.
/// </summary>
public interface IFeatureActivationService
{
    Task EnableAsync(string featureId, CancellationToken cancellationToken = default);
    Task DisableAsync(string featureId, CancellationToken cancellationToken = default);
    Task RemapAsync(string featureId, Type newImplementationType, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default <see cref="IFeatureActivationService"/> backed by an <see cref="IFeatureRegistry"/>.
/// When job / event / MCP tool registries are supplied, <see cref="DisableAsync"/> also
/// sweeps the disabled feature's runtime surface area per SPEC2050 US10 SC02 / SC06 / SC09.
/// </summary>
public sealed class FeatureActivationService : IFeatureActivationService
{
    private readonly IFeatureRegistry _registry;
    private readonly IJobRegistry? _jobs;
    private readonly IEventRouter? _events;
    private readonly IMcpToolRegistry? _mcpTools;
    private readonly IPluginEdgeTypeRegistry? _edgeTypes;

    public FeatureActivationService(IFeatureRegistry registry)
        : this(registry, null, null, null, null) { }

    public FeatureActivationService(
        IFeatureRegistry registry,
        IJobRegistry? jobs,
        IEventRouter? events,
        IMcpToolRegistry? mcpTools)
        : this(registry, jobs, events, mcpTools, null) { }

    public FeatureActivationService(
        IFeatureRegistry registry,
        IJobRegistry? jobs,
        IEventRouter? events,
        IMcpToolRegistry? mcpTools,
        IPluginEdgeTypeRegistry? edgeTypes)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
        _jobs = jobs;
        _events = events;
        _mcpTools = mcpTools;
        _edgeTypes = edgeTypes;
    }

    public Task EnableAsync(string featureId, CancellationToken cancellationToken = default)
    {
        _registry.SetEnabled(featureId, true);
        return Task.CompletedTask;
    }

    public Task DisableAsync(string featureId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(featureId);
        _registry.SetEnabled(featureId, false);
        // SPEC2050 US10 SC02 / SC06 / SC09 / US04 SC01: disabling a feature tears down its runtime surface.
        _jobs?.RemoveByFeature(featureId);
        _events?.RemoveByFeature(featureId);
        _mcpTools?.RemoveByFeature(featureId);
        _edgeTypes?.RemoveByFeature(featureId);
        return Task.CompletedTask;
    }

    public Task RemapAsync(string featureId, Type newImplementationType, CancellationToken cancellationToken = default)
    {
        _registry.Remap(featureId, newImplementationType);
        return Task.CompletedTask;
    }
}
