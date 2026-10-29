namespace LowlandTech.Core.Tests.Refinement;

public abstract class UseCaseScenario<T>(ScenarioFixture<T> fixture) : IAsyncLifetime,
    IClassFixture<ScenarioFixture<T>> where T : IUseCase, new()
{
    protected T UseCase => fixture.UseCase;
    protected T Result => fixture.UseCase;
    protected abstract Task GivenAsync();
    protected abstract Task WhenAsync();
    public ValueTask InitializeAsync() => new(fixture.ExecuteAsync(GivenAsync, WhenAsync));
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
