namespace LowlandTech.Core.Tests.CORE0001.CoreContracts.US01.Registry;

[Trait("CORE", "0001"), Trait("US", "01"), Trait("SC", "01")]
[UserStory("01", "The contracts of the canonical producer")]
public sealed class SC01_TheRegistryKeepsTheIdentityOfItsDescriptors(global::LowlandTech.Core.Tests.Refinement.ScenarioFixture<UseCases.CoreContractUseCase> fixture) : global::LowlandTech.Core.Tests.Refinement.UseCaseScenario<UseCases.CoreContractUseCase>(fixture)
{
    [Given("the production registry types of the canonical producer")]
    protected override Task GivenAsync() => UseCase.ArrangeAsync();

    [When("the registry is built and its descriptors are read")]
    protected override Task WhenAsync() => UseCase.InvokeAsync();

    [Then("Result.RegistryPreservesOrder should be true")]
    [Trait("UAC", "01"), Fact(DisplayName = "UAC01: the registry should preserve the order")]
    public void UAC01_RegistryPreservesOrder() => Result.RegistryPreservesOrder.ShouldBe(true, Result.Diagnostic("RegistryPreservesOrder"));

    [Then("Result.RegistryFindsByEntityType should be true")]
    [Trait("UAC", "02"), Fact(DisplayName = "UAC02: the registry should find the descriptor of an entity type")]
    public void UAC02_RegistryFindsByEntityType() => Result.RegistryFindsByEntityType.ShouldBe(true, Result.Diagnostic("RegistryFindsByEntityType"));

    [Then("Result.EntityProjectionIsStatic should be true")]
    [Trait("UAC", "03"), Fact(DisplayName = "UAC03: the entity projection should be static")]
    public void UAC03_EntityProjectionIsStatic() => Result.EntityProjectionIsStatic.ShouldBe(true, Result.Diagnostic("EntityProjectionIsStatic"));

    [Then("Result.DuplicateDescriptorsAreRefused should be true")]
    [Trait("UAC", "04"), Fact(DisplayName = "UAC04: the duplicate descriptors should be refused")]
    public void UAC04_DuplicateDescriptorsAreRefused() => Result.DuplicateDescriptorsAreRefused.ShouldBe(true, Result.Diagnostic("DuplicateDescriptorsAreRefused"));

    [Then("Result.PluginDefaultIsRouted should be true")]
    [Trait("UAC", "05"), Fact(DisplayName = "UAC05: the plugin default should be routed")]
    public void UAC05_PluginDefaultIsRouted() => Result.PluginDefaultIsRouted.ShouldBe(true, Result.Diagnostic("PluginDefaultIsRouted"));
}
