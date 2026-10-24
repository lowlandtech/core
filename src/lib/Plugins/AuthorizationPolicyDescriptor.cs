namespace LowlandTech.Core.Plugins;

public sealed record AuthorizationPolicyDescriptor(
    string PluginId,
    string FeatureId,
    string PolicyName,
    Type ContributionType,
    string[] UsesPermissions);
