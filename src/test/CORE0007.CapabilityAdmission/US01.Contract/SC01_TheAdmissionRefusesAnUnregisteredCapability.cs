namespace LowlandTech.Core.Tests.CORE0007.CapabilityAdmission.US01.Contract;

[Trait("CORE", "0007"), Trait("US", "01"), Trait("SC", "01")]
[UserStory("01", "The capability admission of the canonical contract")]
public sealed class SC01_TheAdmissionRefusesAnUnregisteredCapability(global::LowlandTech.Core.Tests.Refinement.ScenarioFixture<UseCases.CapabilityAdmissionUseCase> fixture) : global::LowlandTech.Core.Tests.Refinement.UseCaseScenario<UseCases.CapabilityAdmissionUseCase>(fixture)
{
    [Given("the host document that declares a capability")]
    protected override Task GivenAsync() => UseCase.ArrangeAsync();

    [When("the host admits the document with its registration")]
    protected override Task WhenAsync() => UseCase.InvokeAsync();

    [Then("Result.EmptyRegistrationRefusesTheDeclaredCapability should be true")]
    [Trait("UAC", "01"), Fact(DisplayName = "UAC01: an empty registration should refuse the declared capability")]
    public void UAC01_EmptyRegistrationRefusesTheDeclaredCapability() => Result.EmptyRegistrationRefusesTheDeclaredCapability.ShouldBe(true, Result.Diagnostic("EmptyRegistrationRefusesTheDeclaredCapability"));

    [Then("Result.AbsentRegistrationLeavesTheCapabilityUnconstrained should be true")]
    [Trait("UAC", "02"), Fact(DisplayName = "UAC02: an absent registration should leave the capability unconstrained")]
    public void UAC02_AbsentRegistrationLeavesTheCapabilityUnconstrained() => Result.AbsentRegistrationLeavesTheCapabilityUnconstrained.ShouldBe(true, Result.Diagnostic("AbsentRegistrationLeavesTheCapabilityUnconstrained"));

    [Then("Result.TheCanonicalBytesAreUnchanged should be true")]
    [Trait("UAC", "03"), Fact(DisplayName = "UAC03: the canonical bytes of the document should be unchanged")]
    public void UAC03_TheCanonicalBytesAreUnchanged() => Result.TheCanonicalBytesAreUnchanged.ShouldBe(true, Result.Diagnostic("TheCanonicalBytesAreUnchanged"));

    [Then("Result.ARegisteredCapabilityIsAdmitted should be true")]
    [Trait("UAC", "04"), Fact(DisplayName = "UAC04: a registered capability should be admitted")]
    public void UAC04_ARegisteredCapabilityIsAdmitted() => Result.ARegisteredCapabilityIsAdmitted.ShouldBe(true, Result.Diagnostic("ARegisteredCapabilityIsAdmitted"));

    [Then("Result.TheUnchangedRefusalsStillHold should be true")]
    [Trait("UAC", "05"), Fact(DisplayName = "UAC05: the unchanged refusals of the v1 contract should still hold")]
    public void UAC05_TheUnchangedRefusalsStillHold() => Result.TheUnchangedRefusalsStillHold.ShouldBe(true, Result.Diagnostic("TheUnchangedRefusalsStillHold"));
}
