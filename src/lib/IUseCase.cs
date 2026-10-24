namespace LowlandTech.Core;

public interface IUseCase
{
    string Name { get; }
    Task SeedAsync(CancellationToken cancellationToken = default);
    Task RemoveAsync(CancellationToken cancellationToken = default);
}
