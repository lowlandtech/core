namespace LowlandTech.Core.UseCases;

public static class UseCaseManagerReducer
{
    public static UseCaseManagerState Reduce(UseCaseManagerState state, object action) =>
        action switch
        {
            LoadUseCases => state with { Status = "loading", Error = null },
            LoadUseCasesSucceeded loaded => state with
            {
                Status = "idle",
                Items = loaded.Items,
                Page = ClampPage(state.Page, loaded.Items.Count, state.PageSize),
                Error = null,
            },
            LoadUseCasesFailed failed => state with
            {
                Status = "error",
                Error = failed.Error,
            },
            SeedUseCase seed => state with
            {
                Status = "seeding",
                SeedingId = seed.Id,
                Error = null,
            },
            SeedUseCaseSucceeded seeded => state with
            {
                Status = "idle",
                SeedingId = null,
                Items = state.Items
                    .Select(item => item.Id == seeded.Item.Id ? seeded.Item : item)
                    .ToList(),
            },
            SeedUseCaseFailed failed => state with
            {
                Status = "error",
                SeedingId = null,
                Error = failed.Error,
                Items = state.Items
                    .Select(item => item.Id == failed.Id
                        ? item with { IsSeeded = false, LastError = failed.Error }
                        : item)
                    .ToList(),
            },
            NextUseCasePage => state with
            {
                Page = Math.Min(state.Page + 1, state.TotalPages),
            },
            PreviousUseCasePage => state with
            {
                Page = Math.Max(state.Page - 1, 1),
            },
            ChangeUseCasePageSize changed => state with
            {
                Page = 1,
                PageSize = Math.Max(1, changed.PageSize),
            },
            _ => state,
        };

    private static int ClampPage(int page, int totalCount, int pageSize)
    {
        var totalPages = totalCount == 0
            ? 1
            : (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize));
        return Math.Clamp(page, 1, totalPages);
    }
}
