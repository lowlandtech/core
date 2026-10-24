namespace LowlandTech.Core.UseCases;

public sealed record UseCaseManagerState
{
    public string Status { get; init; } = "idle";
    public IReadOnlyList<UseCaseStatus> Items { get; init; } = [];
    public Guid? SeedingId { get; init; }
    public string? Error { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public int TotalCount => Items.Count;
    public int TotalPages => TotalCount == 0
        ? 1
        : (int)Math.Ceiling(TotalCount / (double)Math.Max(1, PageSize));
    public IReadOnlyList<UseCaseStatus> PagedItems => Items
        .Skip((Page - 1) * PageSize)
        .Take(PageSize)
        .ToList();
}
