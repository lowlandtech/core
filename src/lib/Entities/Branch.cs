namespace LowlandTech.Domain.Entities;

public class Branch
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Remote { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsWorktree { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public int Port { get; set; }
    public Guid WorkspaceId { get; set; }

    public Workspace Workspace { get; set; } = null!;
}
