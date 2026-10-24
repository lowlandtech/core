namespace LowlandTech.Core.Plugins;

/// <summary>
/// Tracks authorization policies contributed by plugins.
/// SPEC2050 US13.
/// </summary>
public interface IAuthorizationPolicyRegistry
{
    IReadOnlyCollection<AuthorizationPolicyDescriptor> All { get; }

    /// <summary>Register a policy contribution.</summary>
    /// <exception cref="AuthorizationPolicyConflictException">Another plugin already registered the same policy name.</exception>
    void Register(AuthorizationPolicyDescriptor descriptor);

    /// <summary>Remove a policy by name (called on feature disable).</summary>
    bool Unregister(string policyName);

    bool IsRegistered(string policyName);
}
