namespace LowlandTech.Core.Abstractions;

/// <summary>
/// Contract for a use case.
/// </summary>
public interface IUseCase
{
    /// <summary>
    /// Execute the use case.
    /// </summary>
    /// <returns></returns>
    Task SeedAsync();
}
