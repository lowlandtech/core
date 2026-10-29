namespace LowlandTech.Core.Tests.Refinement;

public interface IUseCase
{
    IReadOnlyList<ResultCase> ResultCases { get; }
    Task ArrangeAsync();
    Task InvokeAsync();
}
