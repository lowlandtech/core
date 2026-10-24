
namespace LowlandTech.Core.Plugins;

public sealed class AuthenticationSchemeRegistry
    : DescriptorRegistry<string, AuthenticationSchemeDescriptor>, IAuthenticationSchemeRegistry
{
    public AuthenticationSchemeRegistry() : base(StringComparer.OrdinalIgnoreCase) { }

    protected override string KeyOf(AuthenticationSchemeDescriptor descriptor) => descriptor.SchemeName;

    protected override void OnDuplicate(string key, AuthenticationSchemeDescriptor existing, AuthenticationSchemeDescriptor incoming)
        => throw new AuthenticationSchemeConflictException(key, existing.PluginId, incoming.PluginId);
}
