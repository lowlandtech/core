namespace LowlandTech.Core.Plugins;

/// <summary>
/// Outcome returned by <see cref="DevelopPromotionGate.CheckAsync"/>.
/// SPEC2050 US05 SC10.
/// </summary>
public sealed record PromotionGateResult
{
    public bool IsReleasable { get; init; }
    public IReadOnlyList<string> PassedUseCases { get; init; } = [];
    public IReadOnlyList<string> FailedUseCases { get; init; } = [];
    public IReadOnlyList<string> Errors { get; init; } = [];
}
