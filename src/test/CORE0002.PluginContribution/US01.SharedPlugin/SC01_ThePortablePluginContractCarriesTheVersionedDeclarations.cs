namespace LowlandTech.Core.Tests.CORE0002.PluginContribution.US01.SharedPlugin;

[Trait("CORE", "0002"), Trait("US", "01"), Trait("SC", "01")]
[UserStory("01", "The portable plugin contribution contract")]
public sealed class SC01_ThePortablePluginContractCarriesTheVersionedDeclarations(global::LowlandTech.Core.Tests.Refinement.ScenarioFixture<UseCases.PluginContributionUseCase> fixture) : global::LowlandTech.Core.Tests.Refinement.UseCaseScenario<UseCases.PluginContributionUseCase>(fixture)
{
    [Given("the manifest documents the Plugins lane reported")]
    protected override Task GivenAsync() => UseCase.ArrangeAsync();

    [When("the portable declaration reads, writes and refuses them")]
    protected override Task WhenAsync() => UseCase.InvokeAsync();

    [Then("Result.PortableDeclarationsCarryNoRuntimeTypeNames should be true")]
    [Trait("UAC", "01"), Fact(DisplayName = "UAC01: the portable declarations should carry no runtime type names")]
    public void UAC01_PortableDeclarationsCarryNoRuntimeTypeNames() => Result.PortableDeclarationsCarryNoRuntimeTypeNames.ShouldBe(true, Result.Diagnostic("PortableDeclarationsCarryNoRuntimeTypeNames"));

    [Then("Result.ConsumersUseOneVersionedContract should be true")]
    [Trait("UAC", "02"), Fact(DisplayName = "UAC02: the consumers should use one versioned contract")]
    public void UAC02_ConsumersUseOneVersionedContract() => Result.ConsumersUseOneVersionedContract.ShouldBe(true, Result.Diagnostic("ConsumersUseOneVersionedContract"));

    [Then("Result.PackageOnlyConsumerValidatesDeclarations should be true")]
    [Trait("UAC", "03"), Fact(DisplayName = "UAC03: the package-only consumer should validate the declarations")]
    public void UAC03_PackageOnlyConsumerValidatesDeclarations() => Result.PackageOnlyConsumerValidatesDeclarations.ShouldBe(true, Result.Diagnostic("PackageOnlyConsumerValidatesDeclarations"));

    [Then("Result.ContractHasNoHostOrEfDependencies should be true")]
    [Trait("UAC", "04"), Fact(DisplayName = "UAC04: the contract should carry no host or entity-framework dependency")]
    public void UAC04_ContractHasNoHostOrEfDependencies() => Result.ContractHasNoHostOrEfDependencies.ShouldBe(true, Result.Diagnostic("ContractHasNoHostOrEfDependencies"));
}
