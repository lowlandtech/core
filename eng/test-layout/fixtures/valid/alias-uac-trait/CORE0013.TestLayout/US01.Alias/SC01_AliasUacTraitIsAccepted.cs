namespace LowlandTech.Core.Tests.CORE0013.TestLayout.US01.Alias;

[Trait("CORE", "0013"), Trait("US", "01"), Trait("SC", "01")]
[UserStory("01", "Alias fixture")]
public sealed class SC01_AliasUacTraitIsAccepted(global::LowlandTech.Core.Tests.Refinement.ScenarioFixture<UseCases.FixtureUseCase> fixture) : global::LowlandTech.Core.Tests.Refinement.UseCaseScenario<UseCases.FixtureUseCase>(fixture)
{
    [Given("the canonical alias of the trait key")]
    protected override Task GivenAsync() => UseCase.ArrangeAsync();

    [When("the fixture is invoked")]
    protected override Task WhenAsync() => UseCase.InvokeAsync();

    [Then("Result.Observed should be true")]
    [Trait(Spec.UAC, "01"), Fact(DisplayName = "UAC01: observed")]
    public void UAC01_Observed() => Result.Observed.ShouldBe(true);
}
