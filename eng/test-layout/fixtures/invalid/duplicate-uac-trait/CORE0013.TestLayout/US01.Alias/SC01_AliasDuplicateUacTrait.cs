namespace LowlandTech.Core.Tests.CORE0013.TestLayout.US01.Alias;

[Trait("CORE", "0013"), Trait("US", "01"), Trait("SC", "01")]
[UserStory("01", "Alias fixture")]
public sealed class SC01_AliasDuplicateUacTrait(global::LowlandTech.Core.Tests.Refinement.ScenarioFixture<UseCases.FixtureUseCase> fixture) : global::LowlandTech.Core.Tests.Refinement.UseCaseScenario<UseCases.FixtureUseCase>(fixture)
{
    [Given("a fixture arrangement")]
    protected override Task GivenAsync() => UseCase.ArrangeAsync();

    [When("the fixture is invoked")]
    protected override Task WhenAsync() => UseCase.InvokeAsync();

    [Then("Result.Observed should be true")]
    [Trait(Spec.UAC, "04"), Fact(DisplayName = "UAC04: observed once")]
    public void UAC04_ObservedOnce() => Result.Observed.ShouldBe(true);

    [Then("Result.Observed should be true")]
    [Trait(Spec.UAC, "04"), Fact(DisplayName = "UAC04: observed again")]
    public void UAC04_ObservedAgain() => Result.Observed.ShouldBe(true);
}
