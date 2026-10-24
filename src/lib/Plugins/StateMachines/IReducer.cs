namespace LowlandTech.Core.Plugins.StateMachines;

/// <summary>
/// Pure state transition function. Matches the "REDUCER TRANSITIONS" block of an
/// Agentic Gherkin state machine: one case per <typeparamref name="TAction"/> type.
/// SPEC2050 — template for every spec carrying a state machine (US04, US14, US20, US08, etc.).
/// </summary>
public interface IReducer<TState, TAction>
{
    TState Reduce(TState state, TAction action);
}
