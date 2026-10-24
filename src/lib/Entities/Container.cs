namespace LowlandTech.Domain.Entities;

public class Container
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Component";
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    /// <summary>
    /// JSON blob holding the SPEC2100.US19 type-definition form values
    /// (Description, Icon, Tags, type-specific fields). Null until the
    /// user edits the canvas's type-definition for the first time.
    /// </summary>
    public string? TypeMetadata { get; set; }

    // SPEC2100.US05 — CSS Grid container template, surfaced through the
    // canvas-mode Properties accordion. Promoted to first-class columns
    // (rather than buried in TypeMetadata) because the layout descriptor
    // queries them on every render.
    public string? GridTemplateColumns { get; set; }
    public string? GridTemplateRows { get; set; }
    public string? GridTemplateAreas { get; set; }
    public string? GridGap { get; set; }

    public Guid? ThemeId { get; set; }
    public Theme? Theme { get; set; }

    public Guid? RazorTemplateId { get; set; }
    public Template? RazorTemplate { get; set; }

    public ICollection<ContainerItem> Items { get; set; } = [];
}
