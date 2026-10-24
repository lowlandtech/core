namespace LowlandTech.Domain.Entities;

public class TenantMember
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";

    public Tenant Tenant { get; set; } = null!;
}
