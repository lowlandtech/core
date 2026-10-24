namespace LowlandTech.Core.Plugins;

/// <summary>
/// Caller-side claim/permission probe consulted by <see cref="FeatureGate"/>.
/// </summary>
public interface IFeaturePrincipal
{
    bool HasPermission(string permission);
}

/// <summary>
/// Enforces feature-level RBAC: callers can only invoke a feature's contract if the
/// feature is registered, enabled, and the principal carries every required permission.
/// SPEC2050 US03 SC05/SC06.
/// </summary>
public sealed class FeatureGate(IFeatureRegistry registry, IFeaturePrincipal principal)
{
    public void EnsureAccess(string featureId)
    {
        ArgumentNullException.ThrowIfNull(featureId);

        var feature = registry.Find(featureId)
            ?? throw new FeatureNotRegisteredException(featureId);

        if (!feature.IsEnabled)
            throw new FeatureDisabledException(featureId);

        foreach (var permission in feature.RequiredPermissions)
        {
            if (!principal.HasPermission(permission))
                throw new FeatureAccessDeniedException(featureId, permission);
        }
    }
}
