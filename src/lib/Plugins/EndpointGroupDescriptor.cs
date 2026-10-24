namespace LowlandTech.Core.Plugins;

/// <summary>
/// Snapshot of an <see cref="EndpointGroup"/> registration.
/// </summary>
public sealed record EndpointGroupDescriptor(
    string FeatureId,
    Type GroupType,
    string GroupPrefix,
    string[] Tags,
    string? RequiredAuthenticationScheme,
    string? RequiredPolicy);
