namespace LowlandTech.Core.Plugins;

/// <summary>
/// Filtered enumeration of registered features (default: only enabled ones).
/// SPEC2050 US03.
/// </summary>
public interface IFeatureDiscoveryService
{
    /// <summary>Returns enabled features, optionally narrowed by an additional predicate.</summary>
    IReadOnlyCollection<FeatureDescriptor> Discover(Predicate<FeatureDescriptor>? filter = null);
}

/// <summary>Default <see cref="IFeatureDiscoveryService"/> backed by an <see cref="IFeatureRegistry"/>.</summary>
public sealed class FeatureDiscoveryService(IFeatureRegistry registry) : IFeatureDiscoveryService
{
    public IReadOnlyCollection<FeatureDescriptor> Discover(Predicate<FeatureDescriptor>? filter = null)
    {
        IEnumerable<FeatureDescriptor> enabled = registry.All.Where(f => f.IsEnabled);
        if (filter is not null) enabled = enabled.Where(f => filter(f));
        return enabled.ToArray();
    }
}
