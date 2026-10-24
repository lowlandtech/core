namespace LowlandTech.Domain.Entities;

public class PostItem
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int SortOrder { get; set; }
    public string ItemType { get; set; } = "Paragraph";
    public string Content { get; set; } = string.Empty;
    public string? Properties { get; set; }

    public Post Post { get; set; } = null!;
}
