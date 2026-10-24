namespace LowlandTech.Domain.Entities;

/// <summary>
/// Represents a Scenario (SC##) within a <see cref="Feature"/> (US##).
/// Seeded by <see cref="LowlandTech.Domain.UseCases.FeatureBuilder"/>.
/// </summary>
public class Scenario
{
    public Guid Id { get; set; }
    public Guid FeatureId { get; set; }
    public int ScNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;   // e.g. "ListInvoices"
    public string? StateSlice { get; set; }                 // primary @state: slice
    public string? ActionName { get; set; }                 // primary @action: name
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public Feature Feature { get; set; } = null!;
}
