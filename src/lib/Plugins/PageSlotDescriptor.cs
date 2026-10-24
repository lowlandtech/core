namespace LowlandTech.Core.Plugins;

/// <summary>
/// Declarative description of a named page slot from an SC00 page contract.
/// SPEC2050 US06.
/// </summary>
public sealed record PageSlotDescriptor
{
    public required string Name { get; init; }
    public string? Component { get; init; }
    public string? Description { get; init; }
    public int Order { get; init; }
}
