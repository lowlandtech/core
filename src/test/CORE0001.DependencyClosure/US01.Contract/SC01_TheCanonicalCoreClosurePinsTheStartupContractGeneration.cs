namespace LowlandTech.Core.Tests.CORE0001.DependencyClosure.US01.Contract;

[Trait("CORE", "0001"), Trait("US", "01"), Trait("SC", "01")]
[UserStory("01", "The dependency closure of the canonical producer")]
public sealed class SC01_TheCanonicalCoreClosurePinsTheStartupContractGeneration(global::LowlandTech.Core.Tests.Refinement.ScenarioFixture<UseCases.DependencyClosureUseCase> fixture) : global::LowlandTech.Core.Tests.Refinement.UseCaseScenario<UseCases.DependencyClosureUseCase>(fixture)
{
    [Given("the repository owned closure gate and the local feed")]
    protected override Task GivenAsync() => UseCase.ArrangeAsync();

    [When("the gate reads the pin of the producer and the versions of the feed")]
    protected override Task WhenAsync() => UseCase.InvokeAsync();

    [Then("Result.ProducerDeclaresExactPin should be true")]
    [Trait("UAC", "02"), Fact(DisplayName = "UAC02: the producer should declare an exact pin")]
    public void UAC02_ProducerDeclaresExactPin() => Result.ProducerDeclaresExactPin.ShouldBe(true, Result.Diagnostic("ProducerDeclaresExactPin"));

    [Then("Result.PinIsUnambiguous should be true")]
    [Trait("UAC", "03"), Fact(DisplayName = "UAC03: the pin should be unambiguous")]
    public void UAC03_PinIsUnambiguous() => Result.PinIsUnambiguous.ShouldBe(true, Result.Diagnostic("PinIsUnambiguous"));

    [Then("Result.PinnedGenerationCarriesTheStartupContract should be true")]
    [Trait("UAC", "04"), Fact(DisplayName = "UAC04: the pinned generation should carry the startup contract")]
    public void UAC04_PinnedGenerationCarriesTheStartupContract() => Result.PinnedGenerationCarriesTheStartupContract.ShouldBe(true, Result.Diagnostic("PinnedGenerationCarriesTheStartupContract"));

    [Then("Result.PublishedCandidateDeclaresThePin should be true")]
    [Trait("UAC", "05"), Fact(DisplayName = "UAC05: the published candidate should declare the pin")]
    public void UAC05_PublishedCandidateDeclaresThePin() => Result.PublishedCandidateDeclaresThePin.ShouldBe(true, Result.Diagnostic("PublishedCandidateDeclaresThePin"));

    [Then("Result.DivergencesHaveOwners should be true")]
    [Trait("UAC", "06"), Fact(DisplayName = "UAC06: the divergences should name their owners")]
    public void UAC06_DivergencesHaveOwners() => Result.DivergencesHaveOwners.ShouldBe(true, Result.Diagnostic("DivergencesHaveOwners"));
}
