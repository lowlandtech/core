
namespace LowlandTech.Core.Plugins;

/// <summary>
/// Checks whether develop is releasable by running every <see cref="IUseCase.SeedAsync"/>
/// idempotently and collecting any failures.
/// SPEC2050 US05 SC10.
/// </summary>
public sealed class DevelopPromotionGate(IEnumerable<IUseCase> useCases)
{
    private readonly IReadOnlyList<IUseCase> _useCases = useCases?.ToList() ?? [];

    /// <summary>
    /// Run all use case seeds. Returns a <see cref="PromotionGateResult"/> indicating
    /// whether develop is releasable.
    /// </summary>
    public async Task<PromotionGateResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        var passed = new List<string>();
        var failed = new List<string>();
        var errors = new List<string>();

        foreach (var useCase in _useCases)
        {
            try
            {
                await useCase.SeedAsync(cancellationToken).ConfigureAwait(false);
                passed.Add(useCase.Name);
            }
            catch (Exception ex)
            {
                failed.Add(useCase.Name);
                errors.Add($"{useCase.Name}: {ex.Message}");
            }
        }

        return new PromotionGateResult
        {
            IsReleasable = failed.Count == 0,
            PassedUseCases = passed,
            FailedUseCases = failed,
            Errors = errors,
        };
    }
}
