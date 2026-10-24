namespace LowlandTech.Domain.Entities;

public class ContainerItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public Guid ContainerId { get; set; }

    // Element metadata
    public string? Element { get; set; }       // HTML element: "button", "div", "input", "a"
    public string? ElementType { get; set; }   // Contract type: "FormControl", "LayoutContainer", etc.

    // Hierarchy
    public Guid? ParentId { get; set; }

    // Grid/Layout
    public bool IsGrid { get; set; }

    // Grid definition (when IsGrid: this item hosts a grid of `Columns` × `Rows` cells)
    public int Columns { get; set; } = 12;
    public int Rows { get; set; } = 8;

    // Grid positioning (when this item is a child in a grid parent)
    public int? GridColumnStart { get; set; }
    public int? GridColumnEnd { get; set; }
    public int? GridRowStart { get; set; }
    public int? GridRowEnd { get; set; }

    // Navigation
    public Container Container { get; set; } = null!;
    public ContainerItem? Parent { get; set; }
    public ICollection<ContainerItem> Children { get; set; } = [];
    public ICollection<ContainerAttribute> Attributes { get; set; } = [];
}
