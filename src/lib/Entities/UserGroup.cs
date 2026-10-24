namespace LowlandTech.Domain.Entities;

public class UserGroup
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;

    public Tenant Tenant { get; set; } = null!;
    public ICollection<UserGroupMember> Members { get; set; } = [];
    public ICollection<Workspace> Workspaces { get; set; } = [];
}
