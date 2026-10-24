namespace LowlandTech.Core.Plugins;

/// <summary>Two plugin packages tried to register the same id.</summary>
public sealed class DuplicatePluginIdException(string pluginId)
    : Exception($"A plugin with id '{pluginId}' is already registered.")
{
    public string PluginId { get; } = pluginId;
}

/// <summary>A plugin's <see cref="PluginDependency"/> references an id that is not installed.</summary>
public sealed class PluginDependencyMissingException(string dependentId, string missingId)
    : Exception($"Plugin '{dependentId}' depends on '{missingId}' which is not installed.")
{
    public string DependentId { get; } = dependentId;
    public string MissingId { get; } = missingId;
}

/// <summary>An installed plugin's version does not satisfy a dependent's required range.</summary>
public sealed class PluginVersionIncompatibleException(
    string dependentId,
    string dependencyId,
    string requiredMin,
    string? requiredMax,
    string installedVersion)
    : Exception(
        $"Plugin '{dependentId}' requires '{dependencyId}' " +
        $">= {requiredMin}{(requiredMax is null ? "" : $" and <= {requiredMax}")}, " +
        $"but installed version is {installedVersion}.")
{
    public string DependentId { get; } = dependentId;
    public string DependencyId { get; } = dependencyId;
    public string RequiredMin { get; } = requiredMin;
    public string? RequiredMax { get; } = requiredMax;
    public string InstalledVersion { get; } = installedVersion;
}

/// <summary>The plugin dependency graph contains a cycle.</summary>
public sealed class PluginDependencyCycleException(IReadOnlyList<string> cyclePath)
    : Exception($"Plugin dependency cycle detected: {string.Join(" → ", cyclePath)}")
{
    public IReadOnlyList<string> CyclePath { get; } = cyclePath;
}

/// <summary>Cannot uninstall a plugin while other plugins depend on it.</summary>
public sealed class PluginHasDependentsException(string pluginId, IReadOnlyList<string> dependentIds)
    : Exception($"Cannot uninstall '{pluginId}': dependent plugins still installed: {string.Join(", ", dependentIds)}")
{
    public string PluginId { get; } = pluginId;
    public IReadOnlyList<string> DependentIds { get; } = dependentIds;
}

/// <summary>An <see cref="EndpointGroup"/> is registered with a prefix already in use.</summary>
public sealed class EndpointPrefixConflictException(string prefix, string existingFeatureId, string newFeatureId)
    : Exception($"Endpoint prefix '{prefix}' is already registered by feature '{existingFeatureId}'; '{newFeatureId}' cannot reuse it.")
{
    public string Prefix { get; } = prefix;
    public string ExistingFeatureId { get; } = existingFeatureId;
    public string NewFeatureId { get; } = newFeatureId;
}

/// <summary>Two plugins tried to register the same authentication scheme name.</summary>
public sealed class AuthenticationSchemeConflictException(string schemeName, string existingPluginId, string newPluginId)
    : Exception($"Authentication scheme '{schemeName}' is already registered by plugin '{existingPluginId}'; '{newPluginId}' cannot reuse it.")
{
    public string SchemeName { get; } = schemeName;
    public string ExistingPluginId { get; } = existingPluginId;
    public string NewPluginId { get; } = newPluginId;
}

/// <summary>Two plugins tried to register the same authorization policy name.</summary>
public sealed class AuthorizationPolicyConflictException(string policyName, string existingPluginId, string newPluginId)
    : Exception($"Authorization policy '{policyName}' is already registered by plugin '{existingPluginId}'; '{newPluginId}' cannot reuse it.")
{
    public string PolicyName { get; } = policyName;
    public string ExistingPluginId { get; } = existingPluginId;
    public string NewPluginId { get; } = newPluginId;
}

/// <summary>An endpoint references an authorization policy that no plugin has registered.</summary>
public sealed class UnknownAuthorizationPolicyException(string policyName, string requestingFeatureId)
    : Exception($"Feature '{requestingFeatureId}' references authorization policy '{policyName}' which is not registered by any plugin.")
{
    public string PolicyName { get; } = policyName;
    public string RequestingFeatureId { get; } = requestingFeatureId;
}

/// <summary>Two plugins claim to own the same entity type in their context descriptors.</summary>
public sealed class AmbiguousEntityOwnerException(Type entityType, string existingPluginId, string newPluginId)
    : Exception($"Entity type '{entityType.FullName}' is already claimed by plugin '{existingPluginId}'; '{newPluginId}' cannot also claim it.")
{
    public Type EntityType { get; } = entityType;
    public string ExistingPluginId { get; } = existingPluginId;
    public string NewPluginId { get; } = newPluginId;
}

/// <summary>A caller asked the context factory for a plugin id that has not been registered.</summary>
public sealed class UnknownPluginException : Exception
{
    public UnknownPluginException(string pluginId)
        : base($"No plugin context is registered for plugin id '{pluginId}'.")
    {
        PluginId = pluginId;
    }

    public UnknownPluginException(string pluginId, string message) : base(message)
    {
        PluginId = pluginId;
    }

    public string PluginId { get; }
}

/// <summary>Two features tried to bind the same service contract type.</summary>
public sealed class FeatureContractConflictException(Type contractType, string existingFeatureId, string newFeatureId)
    : Exception($"Service contract '{contractType.FullName}' is already bound by feature '{existingFeatureId}'; '{newFeatureId}' cannot rebind it.")
{
    public Type ContractType { get; } = contractType;
    public string ExistingFeatureId { get; } = existingFeatureId;
    public string NewFeatureId { get; } = newFeatureId;
}

/// <summary>The requested feature id is not present in the feature registry.</summary>
public sealed class FeatureNotRegisteredException(string featureId)
    : Exception($"Feature '{featureId}' is not registered.")
{
    public string FeatureId { get; } = featureId;
}

/// <summary>The requested feature exists but has been disabled.</summary>
public sealed class FeatureDisabledException(string featureId)
    : Exception($"Feature '{featureId}' is currently disabled.")
{
    public string FeatureId { get; } = featureId;
}

/// <summary>The current principal lacks a permission required by a feature.</summary>
public sealed class FeatureAccessDeniedException(string featureId, string missingPermission)
    : Exception($"Access to feature '{featureId}' is denied: missing permission '{missingPermission}'.")
{
    public string FeatureId { get; } = featureId;
    public string MissingPermission { get; } = missingPermission;
}

/// <summary>A descriptor referenced a permission key that no plugin has declared.</summary>
public sealed class UnknownPermissionException(string permissionKey)
    : Exception($"Permission key '{permissionKey}' is not registered by any plugin.")
{
    public string PermissionKey { get; } = permissionKey;
}

/// <summary>A descriptor referenced an i18n resource key that no plugin has declared.</summary>
public sealed class UnknownResourceKeyException(string resourceKey)
    : Exception($"Resource key '{resourceKey}' is not registered by any plugin.")
{
    public string ResourceKey { get; } = resourceKey;
}

/// <summary>A subscription referenced an event type that no publisher has declared.</summary>
public sealed class UnknownEventTypeException(Type eventType)
    : Exception($"Event type '{eventType.FullName}' is not declared by any publisher.")
{
    public Type EventType { get; } = eventType;
}
