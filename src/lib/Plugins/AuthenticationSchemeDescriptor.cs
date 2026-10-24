namespace LowlandTech.Core.Plugins;

public sealed record AuthenticationSchemeDescriptor(
    string PluginId,
    string SchemeName,
    string DisplayName,
    Type ContributionType,
    bool IsDefault = false);
