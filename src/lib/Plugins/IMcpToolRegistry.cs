
namespace LowlandTech.Core.Plugins;

/// <summary>
/// Tracks MCP tool contributions from features. Tools should always be invoked through
/// the <see cref="IGraphEdgeTool{TSource,TTarget}"/> layer (causal integrity).
/// SPEC2050 US10.
/// </summary>
public interface IMcpToolRegistry
{
    IReadOnlyCollection<McpToolDescriptor> All { get; }

    /// <exception cref="DuplicatePluginIdException">A tool with the same name is already registered.</exception>
    void Register(McpToolDescriptor descriptor);

    bool Unregister(string toolName);

    /// <summary>Remove every tool registered by a feature (called on feature disable).</summary>
    void RemoveByFeature(string featureId);

    bool IsRegistered(string toolName);
}

public sealed class McpToolRegistry : DescriptorRegistry<string, McpToolDescriptor>, IMcpToolRegistry
{
    public McpToolRegistry() : base(StringComparer.OrdinalIgnoreCase) { }

    protected override string KeyOf(McpToolDescriptor descriptor) => descriptor.Name;

    protected override void OnDuplicate(string key, McpToolDescriptor existing, McpToolDescriptor incoming)
        => throw new DuplicatePluginIdException(key);

    public void RemoveByFeature(string featureId)
    {
        ArgumentNullException.ThrowIfNull(featureId);
        RemoveWhere(d => string.Equals(d.FeatureId, featureId, StringComparison.OrdinalIgnoreCase));
    }
}
