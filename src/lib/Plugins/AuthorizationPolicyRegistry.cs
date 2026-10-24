
namespace LowlandTech.Core.Plugins;

public sealed class AuthorizationPolicyRegistry
    : DescriptorRegistry<string, AuthorizationPolicyDescriptor>, IAuthorizationPolicyRegistry
{
    public AuthorizationPolicyRegistry() : base(StringComparer.OrdinalIgnoreCase) { }

    protected override string KeyOf(AuthorizationPolicyDescriptor descriptor) => descriptor.PolicyName;

    protected override void OnDuplicate(string key, AuthorizationPolicyDescriptor existing, AuthorizationPolicyDescriptor incoming)
        => throw new AuthorizationPolicyConflictException(key, existing.PluginId, incoming.PluginId);
}
