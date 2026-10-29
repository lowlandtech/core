namespace LowlandTech.Core.Tests.Refinement;

public sealed class ScenarioFixture<T> : IAsyncDisposable where T : IUseCase, new()
{
    private readonly object gate = new();
    private Task? execution;
    public T UseCase { get; } = new();
    public Task ExecuteAsync(Func<Task> given, Func<Task> when)
    {
        lock (gate) { return execution ??= RunAsync(given, when); }
    }
    private static async Task RunAsync(Func<Task> given, Func<Task> when) { await given(); await when(); }
    public async ValueTask DisposeAsync()
    {
        if (UseCase is IAsyncDisposable asyncDisposable) await asyncDisposable.DisposeAsync();
        else if (UseCase is IDisposable disposable) disposable.Dispose();
    }
}
