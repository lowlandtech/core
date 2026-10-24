
namespace LowlandTech.Core.Plugins;

/// <summary>
/// Tracks <see cref="PageTypeDescriptor"/> contributions from features.
/// SPEC2050 US06.
/// </summary>
public interface IPageTypeRegistry
{
    IReadOnlyCollection<PageTypeDescriptor> All { get; }
    PageTypeDescriptor? Find(string pageId);
    void Register(PageTypeDescriptor descriptor);
    bool Unregister(string pageId);
}

public sealed class PageTypeRegistry : DescriptorRegistry<string, PageTypeDescriptor>, IPageTypeRegistry
{
    public PageTypeRegistry() : base(StringComparer.OrdinalIgnoreCase) { }

    protected override string KeyOf(PageTypeDescriptor descriptor) => descriptor.Id;

    protected override void OnDuplicate(string key, PageTypeDescriptor existing, PageTypeDescriptor incoming)
        => throw new DuplicatePluginIdException(key);
}
