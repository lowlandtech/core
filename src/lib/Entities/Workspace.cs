namespace LowlandTech.Domain.Entities;

public class Workspace
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string Organisation { get; set; } = string.Empty;
    public string Assembly { get; set; } = string.Empty;
    public string RootNamespace { get; set; } = string.Empty;
    public string DefaultNamespace { get; set; } = string.Empty;
    public string ContextName { get; set; } = string.Empty;
    public string DefaultBranch { get; set; } = "develop";

    public string? UserId { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? UserGroupId { get; set; }

    public Tenant? Tenant { get; set; }
    public UserGroup? UserGroup { get; set; }

    public ICollection<DataTable> Tables { get; set; } = [];
    public ICollection<Feature> Features { get; set; } = [];
    public ICollection<Workflow> Workflows { get; set; } = [];
    public ICollection<Branch> Branches { get; set; } = [];
    public ICollection<Page> Pages { get; set; } = [];
}
