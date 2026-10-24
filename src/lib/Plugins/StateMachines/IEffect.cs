namespace LowlandTech.Core.Plugins.StateMachines;

/// <summary>
/// Side-effect handler for an action. Matches the "EFFECTS" block of an Agentic
/// Gherkin state machine: one handler per async action. Called AFTER the reducer
/// has produced the new state, so the effect observes the post-transition state.
/// SPEC2050 — template for every spec carrying a state machine.
/// </summary>
public interface IEffect<TState, TAction>
{
    bool CanHandle(TAction action);

    Task HandleAsync(TState state, TAction action, CancellationToken ct);
}
