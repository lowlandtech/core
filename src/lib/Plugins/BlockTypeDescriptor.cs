namespace LowlandTech.Core.Plugins;

/// <summary>
/// Declarative description of a Blazor block (composable UI fragment) contributed by a feature.
/// SPEC2050 US06.
/// </summary>
public sealed record BlockTypeDescriptor
{
    public required string Id { get; init; }
    public required string FeatureId { get; init; }
    public required Type ComponentType { get; init; }
    public required string DisplayName { get; init; }
    public string? Category { get; init; }
    public string? Icon { get; init; }
    public string? Description { get; init; }
}
