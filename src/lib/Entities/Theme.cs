namespace LowlandTech.Domain.Entities;

public class Theme
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
    public ICollection<ThemeToken> Tokens { get; set; } = [];
    public ICollection<ThemeRule> Rules { get; set; } = [];
    public ICollection<Container> Containers { get; set; } = [];
}
