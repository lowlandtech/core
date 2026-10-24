namespace LowlandTech.Domain.Entities;

public class Page
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    /// <summary>The container template this page is based on.</summary>
    public Guid ContainerId { get; set; }
    public Container Container { get; set; } = null!;

    /// <summary>The workspace this page belongs to.</summary>
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;

    /// <summary>Only items that override the container template. Missing items use container defaults.</summary>
    public ICollection<PageItem> Items { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
