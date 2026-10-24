namespace LowlandTech.Core.Plugins;

/// <summary>
/// Declarative description of a background job (IHostedService) contributed by a feature.
/// SPEC2050 US10.
/// </summary>
public sealed record BackgroundJobDescriptor(
    string Id,
    string FeatureId,
    Type JobType,
    string? DisplayName = null,
    string? Description = null);
