namespace LowlandTech.Core.UseCases;

public interface IUseCaseManager
{
    Task<IReadOnlyList<UseCaseStatus>> ListAsync(CancellationToken cancellationToken = default);
    Task<UseCaseStatus?> SeedAsync(Guid id, CancellationToken cancellationToken = default);
    Task SynchronizeAsync(CancellationToken cancellationToken = default);
}
