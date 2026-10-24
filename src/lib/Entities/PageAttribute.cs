namespace LowlandTech.Domain.Entities;

public class PageAttribute
{
    public Guid Id { get; set; }

    /// <summary>The container attribute this overrides. Null means page-only attribute.</summary>
    public Guid? ContainerAttributeId { get; set; }
    public ContainerAttribute? ContainerAttribute { get; set; }

    /// <summary>Override name. Null = use container attribute's name.</summary>
    public string? Name { get; set; }

    /// <summary>The overridden value.</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>Override data type. Null = use container attribute's type.</summary>
    public string? DataType { get; set; }

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public Guid PageItemId { get; set; }
    public PageItem PageItem { get; set; } = null!;
}
