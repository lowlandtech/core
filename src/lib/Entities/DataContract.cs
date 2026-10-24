namespace LowlandTech.Domain.Entities;

public class DataContract
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public ICollection<DataContractRule> Rules { get; set; } = [];
}
