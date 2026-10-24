namespace LowlandTech.Core.Plugins;

/// <summary>
/// Filtered enumeration of block descriptors visible to the block picker — only blocks
/// from enabled features.
/// SPEC2050 US06.
/// </summary>
public interface IBlockDiscoveryService
{
    IReadOnlyCollection<BlockTypeDescriptor> Discover();
}

public sealed class BlockDiscoveryService(
    IBlockTypeRegistry blocks,
    IFeatureRegistry features) : IBlockDiscoveryService
{
    public IReadOnlyCollection<BlockTypeDescriptor> Discover()
    {
        var enabledFeatureIds = new HashSet<string>(
            features.All.Where(f => f.IsEnabled).Select(f => f.Id),
            StringComparer.OrdinalIgnoreCase);

        return blocks.All
            .Where(b => enabledFeatureIds.Contains(b.FeatureId))
            .ToArray();
    }
}
