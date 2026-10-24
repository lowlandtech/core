namespace LowlandTech.Core.UseCases;

public sealed record LoadUseCases;
public sealed record LoadUseCasesSucceeded(IReadOnlyList<UseCaseStatus> Items);
public sealed record LoadUseCasesFailed(string Error);
public sealed record SeedUseCase(Guid Id);
public sealed record SeedUseCaseSucceeded(UseCaseStatus Item);
public sealed record SeedUseCaseFailed(Guid Id, string Error);
public sealed record NextUseCasePage;
public sealed record PreviousUseCasePage;
public sealed record ChangeUseCasePageSize(int PageSize);
