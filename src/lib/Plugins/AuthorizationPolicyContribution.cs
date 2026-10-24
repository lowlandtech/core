namespace LowlandTech.Core.Plugins;

/// <summary>
/// Base class a plugin derives from to contribute a named authorization policy.
/// SPEC2050 US13.
/// </summary>
public abstract class AuthorizationPolicyContribution
{
    public abstract string PolicyName { get; }
}
