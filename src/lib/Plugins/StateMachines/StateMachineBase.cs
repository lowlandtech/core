namespace LowlandTech.Core.Plugins.StateMachines;

/// <summary>
/// Reducer + effects harness matching the 7-section Agentic Gherkin state machine
/// block (STATE / CONFIG / REPRESENTATION / ACTIONS / REDUCER / EFFECTS / EVENTS).
/// Every dispatch runs the reducer, records a <see cref="StateTransition{TState, TAction}"/>,
/// and then invokes any effect that <see cref="IEffect{TState, TAction}.CanHandle"/> the
/// action. Thread-safe: reducer dispatch is serialized on a single gate.
/// SPEC2050 — template for US04 edge transitions, US20 install driver, US08 host lifecycle.
/// </summary>
public abstract class StateMachineBase<TState, TAction>
{
    private readonly IReducer<TState, TAction> _reducer;
    private readonly IReadOnlyList<IEffect<TState, TAction>> _effects;
    private readonly List<StateTransition<TState, TAction>> _log = new();
    private readonly SemaphoreSlim _gate = new(1, 1);

    protected StateMachineBase(
        TState initial,
        IReducer<TState, TAction> reducer,
        IEnumerable<IEffect<TState, TAction>> effects)
    {
        ArgumentNullException.ThrowIfNull(reducer);
        ArgumentNullException.ThrowIfNull(effects);
        State = initial;
        _reducer = reducer;
        _effects = effects.ToArray();
    }

    public TState State { get; private set; }

    public IReadOnlyList<StateTransition<TState, TAction>> Log
    {
        get { lock (_log) return _log.ToArray(); }
    }

    public async Task DispatchAsync(TAction action, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        TState from;
        TState to;
        try
        {
            from = State;
            to = _reducer.Reduce(from, action);
            State = to;
        }
        finally
        {
            _gate.Release();
        }

        string? effectError = null;
        try
        {
            foreach (var effect in _effects)
            {
                if (effect.CanHandle(action))
                    await effect.HandleAsync(to, action, ct).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            effectError = ex.Message;
            lock (_log) _log.Add(new StateTransition<TState, TAction>(DateTimeOffset.UtcNow, from, action, to, Succeeded: false, Error: effectError));
            throw;
        }

        lock (_log) _log.Add(new StateTransition<TState, TAction>(DateTimeOffset.UtcNow, from, action, to, Succeeded: true));
    }
}
