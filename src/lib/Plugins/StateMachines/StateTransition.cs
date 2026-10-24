namespace LowlandTech.Core.Plugins.StateMachines;

/// <summary>
/// One entry in a state machine's audit trail. Captures the
/// <c>(fromState, action, toState)</c> triple required by SPEC2050 US04 SC09.
/// </summary>
public sealed record StateTransition<TState, TAction>(
    DateTimeOffset Timestamp,
    TState FromState,
    TAction Action,
    TState ToState,
    bool Succeeded,
    string? Error = null);
