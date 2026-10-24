
namespace LowlandTech.Domain.Abstractions;

public interface IUserGroupService
{
    Task<List<UserGroup>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<UserGroup?> GetAsync(Guid id, CancellationToken ct = default);
    Task<UserGroup?> UpdateAsync(UserGroup group, CancellationToken ct = default);

    // Admin methods (no authorization — for MCP/system use)
    Task<UserGroup> CreateAsync(UserGroup group, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<UserGroupMember> AddMemberAsync(Guid groupId, string userId, string role = "Member", CancellationToken ct = default);
    Task<bool> RemoveMemberAsync(Guid groupId, string userId, CancellationToken ct = default);

    // Authorized methods (enforce role hierarchy)
    Task<UserGroup> CreateAsAsync(UserGroup group, string callerUserId, CancellationToken ct = default);
    Task<bool> DeleteAsAsync(Guid id, string callerUserId, CancellationToken ct = default);
    Task<UserGroupMember> AddMemberAsAsync(Guid groupId, string userId, string role, string callerUserId, CancellationToken ct = default);
    Task<bool> RemoveMemberAsAsync(Guid groupId, string userId, string callerUserId, CancellationToken ct = default);
    Task<UserGroupMember?> UpdateMemberRoleAsAsync(Guid groupId, string userId, string role, string callerUserId, CancellationToken ct = default);
}
