namespace LowlandTech.Core.Tests.CORE0002.GraphContract.US01.SharedContracts;

[Trait("CORE", "0002"), Trait("US", "01"), Trait("SC", "01")]
[UserStory("01", "The shared transition contract")]
public sealed class SC01_TheSharedContractReadsTheHostDocuments(global::LowlandTech.Core.Tests.Refinement.ScenarioFixture<UseCases.GraphContractUseCase> fixture) : global::LowlandTech.Core.Tests.Refinement.UseCaseScenario<UseCases.GraphContractUseCase>(fixture)
{
    [Given("the Backend parity fixtures of the shared contracts")]
    protected override Task GivenAsync() => UseCase.ArrangeAsync();

    [When("the contract reads the documents and the rules of the contract answer")]
    protected override Task WhenAsync() => UseCase.InvokeAsync();

    [Then("Result.DocumentsRoundTrip should be true")]
    [Trait("UAC", "01"), Fact(DisplayName = "UAC01: the host documents should round trip")]
    public void UAC01_DocumentsRoundTrip() => Result.DocumentsRoundTrip.ShouldBe(true, Result.Diagnostic("DocumentsRoundTrip"));

    [Then("Result.ContractHasOnePackageIdentity should be true")]
    [Trait("UAC", "02"), Fact(DisplayName = "UAC02: the contract should carry one package identity")]
    public void UAC02_ContractHasOnePackageIdentity() => Result.ContractHasOnePackageIdentity.ShouldBe(true, Result.Diagnostic("ContractHasOnePackageIdentity"));

    [Then("Result.PortableFamilyIsComplete should be true")]
    [Trait("UAC", "03"), Fact(DisplayName = "UAC03: the portable family should be complete")]
    public void UAC03_PortableFamilyIsComplete() => Result.PortableFamilyIsComplete.ShouldBe(true, Result.Diagnostic("PortableFamilyIsComplete"));

    [Then("Result.InvalidDefinitionsReceiveNamedRefusals should be true")]
    [Trait("UAC", "04"), Fact(DisplayName = "UAC04: the invalid definitions should receive named refusals")]
    public void UAC04_InvalidDefinitionsReceiveNamedRefusals() => Result.InvalidDefinitionsReceiveNamedRefusals.ShouldBe(true, Result.Diagnostic("InvalidDefinitionsReceiveNamedRefusals"));

    [Then("Result.ContractHasNoHostDependencies should be true")]
    [Trait("UAC", "05"), Fact(DisplayName = "UAC05: the contract should carry no host dependency")]
    public void UAC05_ContractHasNoHostDependencies() => Result.ContractHasNoHostDependencies.ShouldBe(true, Result.Diagnostic("ContractHasNoHostDependencies"));

    [Then("Result.ExistingCorePackageRemainsAvailable should be true")]
    [Trait("UAC", "06"), Fact(DisplayName = "UAC06: the existing Core package should remain available")]
    public void UAC06_ExistingCorePackageRemainsAvailable() => Result.ExistingCorePackageRemainsAvailable.ShouldBe(true, Result.Diagnostic("ExistingCorePackageRemainsAvailable"));
}
