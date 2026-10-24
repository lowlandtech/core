namespace LowlandTech.Core.Plugins;

/// <summary>
/// Records a single effect that ran during the <see cref="TransitionPhase.Effect"/> phase.
/// SPEC2050 US04.
/// </summary>
public sealed record EffectInvocation(
    string Name,
    IReadOnlyDictionary<string, object?> Parameters,
    bool Succeeded,
    string? Error = null);
