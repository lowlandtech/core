namespace LowlandTech.Core.Tests.CORE0004.TestLayout.US01.Fixtures;

[Trait("CORE", "0004"), Trait("US", "01"), Trait("SC", "01")]
[UserStory("01", "Negative fixture")]
public sealed class SC01_NonExpressionFact(global::LowlandTech.Core.Tests.Refinement.ScenarioFixture<UseCases.FixtureUseCase> fixture) : global::LowlandTech.Core.Tests.Refinement.UseCaseScenario<UseCases.FixtureUseCase>(fixture)
{
    [Given("a fixture arrangement")]
    protected override Task GivenAsync() => UseCase.ArrangeAsync();

    [When("the fixture is invoked")]
    protected override Task WhenAsync() => UseCase.InvokeAsync();

    [Then("Result.Observed should be true")]
    [Trait("UAC", "01"), Fact(DisplayName = "UAC01: observed")]
    public void UAC01_Observed()
    {
        Result.Observed.ShouldBe(true);
    }
}
