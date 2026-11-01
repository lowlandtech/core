namespace LowlandTech.Core.Tests.CORE0009.SessionContent.US01.Contract;

[Trait("CORE", "0009"), Trait("US", "01"), Trait("SC", "01")]
[UserStory("01", "The portable session-content contract")]
public sealed class SC01_TheSessionContentContractCarriesThePortableDescriptor(global::LowlandTech.Core.Tests.Refinement.ScenarioFixture<UseCases.SessionContentContractUseCase> fixture) : global::LowlandTech.Core.Tests.Refinement.UseCaseScenario<UseCases.SessionContentContractUseCase>(fixture)
{
    [Given("the contract that a host and a content plugin share")]
    protected override Task GivenAsync() => UseCase.ArrangeAsync();

    [When("the host reads the content of a session through the contract")]
    protected override Task WhenAsync() => UseCase.InvokeAsync();

    [Then("Result.TheDescriptorIsDataOnly should be true")]
    [Trait("UAC", "01"), Fact(DisplayName = "UAC01: the descriptor should be data only")]
    public void UAC01_TheDescriptorIsDataOnly() => Result.TheDescriptorIsDataOnly.ShouldBe(true, Result.Diagnostic("TheDescriptorIsDataOnly"));

    [Then("Result.TheContractCarriesOnlyThePortableSurface should be true")]
    [Trait("UAC", "02"), Fact(DisplayName = "UAC02: the contract should carry the portable surface and no host behaviour")]
    public void UAC02_TheContractCarriesOnlyThePortableSurface() => Result.TheContractCarriesOnlyThePortableSurface.ShouldBe(true, Result.Diagnostic("TheContractCarriesOnlyThePortableSurface"));

    [Then("Result.AHostReadsTheDescriptorThroughTheProvider should be true")]
    [Trait("UAC", "03"), Fact(DisplayName = "UAC03: a host should read the descriptor of a session through the provider")]
    public void UAC03_AHostReadsTheDescriptorThroughTheProvider() => Result.AHostReadsTheDescriptorThroughTheProvider.ShouldBe(true, Result.Diagnostic("AHostReadsTheDescriptorThroughTheProvider"));

    [Then("Result.ThePluginDeclarationSurfaceIsUnchanged should be true")]
    [Trait("UAC", "04"), Fact(DisplayName = "UAC04: the portable plugin declaration surface should be unchanged")]
    public void UAC04_ThePluginDeclarationSurfaceIsUnchanged() => Result.ThePluginDeclarationSurfaceIsUnchanged.ShouldBe(true, Result.Diagnostic("ThePluginDeclarationSurfaceIsUnchanged"));

    [Then("Result.ThePublishedVersionsStayAvailable should be true")]
    [Trait("UAC", "05"), Fact(DisplayName = "UAC05: the published contract versions should stay available and unchanged")]
    public void UAC05_ThePublishedVersionsStayAvailable() => Result.ThePublishedVersionsStayAvailable.ShouldBe(true, Result.Diagnostic("ThePublishedVersionsStayAvailable"));
}
