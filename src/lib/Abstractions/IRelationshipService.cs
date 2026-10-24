
namespace LowlandTech.Domain.Abstractions;

public interface IRelationshipService
{
    Task<List<Relationship>> ListAsync(Guid? workspaceId = null, CancellationToken ct = default);
    Task<Relationship> CreateAsync(Relationship relationship, CancellationToken ct = default);
    Task<Relationship?> UpdateAsync(Relationship relationship, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
