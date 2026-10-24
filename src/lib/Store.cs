namespace LowlandTech.Core;

public class Store<TState> where TState : notnull
{
    private TState _state;
    private readonly Func<TState, object, TState> _reducer;
    private readonly List<Func<Store<TState>, object, Task>> _effects = [];

    public TState State => _state;
    public event Action? StateChanged;

    public Store(TState initialState, Func<TState, object, TState> reducer)
    {
        _state = initialState;
        _reducer = reducer;
    }

    public void Dispatch(object action)
    {
        _state = _reducer(_state, action);
        StateChanged?.Invoke();
    }

    public async Task DispatchAsync(object action)
    {
        Dispatch(action);
        await RunEffectsAsync(action);
    }

    public async Task RunEffectsAsync(object action)
    {
        foreach (var effect in _effects)
        {
            try
            {
                await effect(this, action);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Store effect for action '{action.GetType().Name}' failed: {ex}");
                Console.Error.WriteLine($"Store effect for action '{action.GetType().Name}' failed: {ex}");
            }
        }
    }

    public void RegisterEffect(Func<Store<TState>, object, Task> effect)
        => _effects.Add(effect);
}
