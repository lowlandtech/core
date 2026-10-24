
namespace LowlandTech.Domain.Abstractions;

public interface IPackageService
{
    Task<List<Package>> ListAsync(CancellationToken ct = default);
    Task<Package?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Package> CreateAsync(Package package, CancellationToken ct = default);
    Task<Package?> UpdateAsync(Package package, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<Import> CreateImportAsync(Import import, CancellationToken ct = default);
    Task<Import?> UpdateImportAsync(Import import, CancellationToken ct = default);
    Task<bool> DeleteImportAsync(Guid id, CancellationToken ct = default);
    Task<List<Import>> ListImportsAsync(Guid? packageId = null, CancellationToken ct = default);
}
