namespace LowlandTech.Domain.Entities;

/// <summary>
/// Represents a User Story (US##) within a plugin workspace.
/// Seeded by <see cref="LowlandTech.Domain.UseCases.FeatureBuilder"/>.
/// </summary>
public class Feature
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public int UsNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;   // e.g. "US01.InvoiceManagement"
    public string? Actor { get; set; }
    public string? Want { get; set; }
    public string? SoThat { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public Workspace Workspace { get; set; } = null!;
    public ICollection<Scenario> Scenarios { get; set; } = [];
}
