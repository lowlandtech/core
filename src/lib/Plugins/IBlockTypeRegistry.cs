
namespace LowlandTech.Core.Plugins;

/// <summary>
/// Tracks <see cref="BlockTypeDescriptor"/> contributions from features.
/// SPEC2050 US06.
/// </summary>
public interface IBlockTypeRegistry
{
    IReadOnlyCollection<BlockTypeDescriptor> All { get; }
    BlockTypeDescriptor? Find(string blockId);
    void Register(BlockTypeDescriptor descriptor);
    bool Unregister(string blockId);
}

public sealed class BlockTypeRegistry : DescriptorRegistry<string, BlockTypeDescriptor>, IBlockTypeRegistry
{
    public BlockTypeRegistry() : base(StringComparer.OrdinalIgnoreCase) { }

    protected override string KeyOf(BlockTypeDescriptor descriptor) => descriptor.Id;

    protected override void OnDuplicate(string key, BlockTypeDescriptor existing, BlockTypeDescriptor incoming)
        => throw new DuplicatePluginIdException(key);
}
