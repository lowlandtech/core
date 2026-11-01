namespace LowlandTech.Core.Tests.CORE0008.EndpointGroup.US01.Mount;

[Trait("CORE", "0008"), Trait("US", "01"), Trait("SC", "01")]
[UserStory("01", "The host-owned mount entry point of an endpoint group")]
public sealed class SC01_TheHostMountsADeclaredEndpointGroup(global::LowlandTech.Core.Tests.Refinement.ScenarioFixture<UseCases.EndpointGroupMountUseCase> fixture) : global::LowlandTech.Core.Tests.Refinement.UseCaseScenario<UseCases.EndpointGroupMountUseCase>(fixture)
{
    [Given("a declared endpoint group and a host that owns the route table")]
    protected override Task GivenAsync() => UseCase.ArrangeAsync();

    [When("the host mounts the group through the declared entry point")]
    protected override Task WhenAsync() => UseCase.InvokeAsync();

    [Then("Result.TheEntryPointIsDeclared should be true")]
    [Trait("UAC", "01"), Fact(DisplayName = "UAC01: the canonical contract should declare the entry point and the host context")]
    public void UAC01_TheEntryPointIsDeclared() => Result.TheEntryPointIsDeclared.ShouldBe(true, Result.Diagnostic("TheEntryPointIsDeclared"));

    [Then("Result.TheMetadataStaysInTheDescriptor should be true")]
    [Trait("UAC", "02"), Fact(DisplayName = "UAC02: the declared metadata should stay in the descriptor and the prefix conflict should be refused")]
    public void UAC02_TheMetadataStaysInTheDescriptor() => Result.TheMetadataStaysInTheDescriptor.ShouldBe(true, Result.Diagnostic("TheMetadataStaysInTheDescriptor"));

    [Then("Result.AHostMountsTheDeclaredGroup should be true")]
    [Trait("UAC", "03"), Fact(DisplayName = "UAC03: a host should mount a declared group through the declared entry point")]
    public void UAC03_AHostMountsTheDeclaredGroup() => Result.AHostMountsTheDeclaredGroup.ShouldBe(true, Result.Diagnostic("AHostMountsTheDeclaredGroup"));

    [Then("Result.ThePublishedCoreVersionStaysAvailable should be true")]
    [Trait("UAC", "04"), Fact(DisplayName = "UAC04: the published Core generation should stay available and unchanged")]
    public void UAC04_ThePublishedCoreVersionStaysAvailable() => Result.ThePublishedCoreVersionStaysAvailable.ShouldBe(true, Result.Diagnostic("ThePublishedCoreVersionStaysAvailable"));

    [Then("Result.TheConsumersResolveTheDeclaration should be true")]
    [Trait("UAC", "05"), Fact(DisplayName = "UAC05: the package that the consumers resolve should carry the declaration")]
    public void UAC05_TheConsumersResolveTheDeclaration() => Result.TheConsumersResolveTheDeclaration.ShouldBe(true, Result.Diagnostic("TheConsumersResolveTheDeclaration"));
}
