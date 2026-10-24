
namespace LowlandTech.Domain.Abstractions;

public interface ITenantService
{
    Task<List<Tenant>> ListAsync(CancellationToken ct = default);
    Task<List<Tenant>> ListByUserAsync(string userId, CancellationToken ct = default);
    Task<Tenant?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Tenant> CreateAsync(Tenant tenant, CancellationToken ct = default);
    Task<Tenant?> UpdateAsync(Tenant tenant, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

    // Admin methods (no authorization — for MCP/system use)
    Task<TenantMember> AddMemberAsync(Guid tenantId, string userId, string role = "Member", CancellationToken ct = default);
    Task<bool> RemoveMemberAsync(Guid tenantId, string userId, CancellationToken ct = default);
    Task<TenantMember?> UpdateMemberRoleAsync(Guid tenantId, string userId, string role, CancellationToken ct = default);

    // Authorized methods (enforce role hierarchy)
    Task<Tenant?> UpdateAsAsync(Guid tenantId, Tenant tenant, string callerUserId, CancellationToken ct = default);
    Task<bool> DeleteAsAsync(Guid id, string callerUserId, CancellationToken ct = default);
    Task<TenantMember> AddMemberAsAsync(Guid tenantId, string userId, string role, string callerUserId, CancellationToken ct = default);
    Task<bool> RemoveMemberAsAsync(Guid tenantId, string userId, string callerUserId, CancellationToken ct = default);
    Task<TenantMember?> UpdateMemberRoleAsAsync(Guid tenantId, string userId, string role, string callerUserId, CancellationToken ct = default);
}
