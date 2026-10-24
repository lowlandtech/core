namespace LowlandTech.Core.Plugins;

/// <summary>
/// Declarative description of a cross-feature event subscription contributed by a feature.
/// SPEC2050 US10.
/// </summary>
public sealed record EventSubscriptionDescriptor(
    string FeatureId,
    Type EventType,
    Type HandlerType);
