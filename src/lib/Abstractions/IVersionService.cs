
namespace LowlandTech.Domain.Abstractions;

public interface IVersionService
{
    Task<List<TemplateVersion>> ListAsync(Guid? templateId = null, CancellationToken ct = default);
    Task<TemplateVersion?> GetAsync(Guid id, CancellationToken ct = default);
    Task<TemplateVersion> CreateAsync(TemplateVersion version, CancellationToken ct = default);
    Task<TemplateVersion?> UpdateAsync(TemplateVersion version, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
