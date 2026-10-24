namespace LowlandTech.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<TenantMember> Members { get; set; } = [];
    public ICollection<UserGroup> UserGroups { get; set; } = [];
    public ICollection<Workspace> Workspaces { get; set; } = [];
}
