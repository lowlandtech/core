namespace LowlandTech.Domain.Entities;

public class PageItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ElementType { get; set; } = "LayoutContainer";
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    /// <summary>The container item this overrides. Null means this is a page-only addition.</summary>
    public Guid? ContainerItemId { get; set; }
    public ContainerItem? ContainerItem { get; set; }

    public Guid PageId { get; set; }
    public Page Page { get; set; } = null!;

    // ── Override fields (only set if different from container template) ──

    /// <summary>Override the element. Null = use container item's element.</summary>
    public string? Element { get; set; }

    /// <summary>Override grid flag. Null = use container item's value.</summary>
    public bool? IsGrid { get; set; }

    public int? GridColumnStart { get; set; }
    public int? GridColumnEnd { get; set; }
    public int? GridRowStart { get; set; }
    public int? GridRowEnd { get; set; }

    // Hierarchy within the page
    public Guid? ParentId { get; set; }
    public PageItem? Parent { get; set; }
    public ICollection<PageItem> Children { get; set; } = [];

    /// <summary>Only attributes that override the container item's defaults.</summary>
    public ICollection<PageAttribute> Attributes { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
