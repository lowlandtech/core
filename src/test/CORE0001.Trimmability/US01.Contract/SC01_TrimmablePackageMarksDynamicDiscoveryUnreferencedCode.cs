namespace LowlandTech.Core.Tests.CORE0001.Trimmability.US01.Contract;

[Trait("CORE", "0001"), Trait("US", "01"), Trait("SC", "01")]
[UserStory("01", "The trimmable contract of the canonical producer")]
public sealed class SC01_TrimmablePackageMarksDynamicDiscoveryUnreferencedCode(global::LowlandTech.Core.Tests.Refinement.ScenarioFixture<UseCases.TrimmableContractUseCase> fixture) : global::LowlandTech.Core.Tests.Refinement.UseCaseScenario<UseCases.TrimmableContractUseCase>(fixture)
{
    [Given("the canonical producer assembly of the published package")]
    protected override Task GivenAsync() => UseCase.ArrangeAsync();

    [When("the assembly is read for its trim marker and its discovery entrypoints")]
    protected override Task WhenAsync() => UseCase.InvokeAsync();

    [Then("Result.AssemblyIsTrimmable should be true")]
    [Trait("UAC", "05"), Fact(DisplayName = "UAC05: the producer assembly should be trimmable")]
    public void UAC05_AssemblyIsTrimmable() => Result.AssemblyIsTrimmable.ShouldBe(true, Result.Diagnostic("AssemblyIsTrimmable"));

    [Then("Result.DiscoveryEntrypointsAreMarked should be true")]
    [Trait("UAC", "06"), Fact(DisplayName = "UAC06: the discovery entrypoints should be marked")]
    public void UAC06_DiscoveryEntrypointsAreMarked() => Result.DiscoveryEntrypointsAreMarked.ShouldBe(true, Result.Diagnostic("DiscoveryEntrypointsAreMarked"));
}
