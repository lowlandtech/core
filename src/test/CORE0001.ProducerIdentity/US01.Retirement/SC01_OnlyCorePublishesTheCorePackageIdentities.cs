namespace LowlandTech.Core.Tests.CORE0001.ProducerIdentity.US01.Retirement;

[Trait("CORE", "0001"), Trait("US", "01"), Trait("SC", "01")]
[UserStory("01", "The producer identity of the canonical checkout")]
public sealed class SC01_OnlyCorePublishesTheCorePackageIdentities(global::LowlandTech.Core.Tests.Refinement.ScenarioFixture<UseCases.ProducerRetirementUseCase> fixture) : global::LowlandTech.Core.Tests.Refinement.UseCaseScenario<UseCases.ProducerRetirementUseCase>(fixture)
{
    [Given("the repository owned producer uniqueness gate")]
    protected override Task GivenAsync() => UseCase.ArrangeAsync();

    [When("the gate reads every project that can declare a Core package identity")]
    protected override Task WhenAsync() => UseCase.InvokeAsync();

    [Then("Result.CanonicalProducersPublish should be true")]
    [Trait("UAC", "01"), Fact(DisplayName = "UAC01: the canonical producers should publish")]
    public void UAC01_CanonicalProducersPublish() => Result.CanonicalProducersPublish.ShouldBe(true, Result.Diagnostic("CanonicalProducersPublish"));

    [Then("Result.NoCoreOwnedViolation should be true")]
    [Trait("UAC", "02"), Fact(DisplayName = "UAC02: the gate should report no Core owned violation")]
    public void UAC02_NoCoreOwnedViolation() => Result.NoCoreOwnedViolation.ShouldBe(true, Result.Diagnostic("NoCoreOwnedViolation"));

    [Then("Result.CompetingProducersHaveOwners should be true")]
    [Trait("UAC", "03"), Fact(DisplayName = "UAC03: the competing producers should name their owners")]
    public void UAC03_CompetingProducersHaveOwners() => Result.CompetingProducersHaveOwners.ShouldBe(true, Result.Diagnostic("CompetingProducersHaveOwners"));

    [Then("Result.RetiredProducersRefuse should be true")]
    [Trait("UAC", "04"), Fact(DisplayName = "UAC04: the retired producers should refuse to publish")]
    public void UAC04_RetiredProducersRefuse() => Result.RetiredProducersRefuse.ShouldBe(true, Result.Diagnostic("RetiredProducersRefuse"));
}
