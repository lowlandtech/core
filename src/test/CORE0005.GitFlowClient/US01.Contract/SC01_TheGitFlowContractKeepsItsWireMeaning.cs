namespace LowlandTech.Core.Tests.CORE0005.GitFlowClient.US01.Contract;

[Trait("CORE", "0005"), Trait("US", "01"), Trait("SC", "01")]
[UserStory("01", "The git-flow client contract")]
public sealed class SC01_TheGitFlowContractKeepsItsWireMeaning(global::LowlandTech.Core.Tests.Refinement.ScenarioFixture<UseCases.GitFlowContractUseCase> fixture) : global::LowlandTech.Core.Tests.Refinement.UseCaseScenario<UseCases.GitFlowContractUseCase>(fixture)
{
    [Given("the git-flow contract of the canonical producer")]
    protected override Task GivenAsync() => UseCase.ArrangeAsync();

    [When("the states, the processor and the proposal are read")]
    protected override Task WhenAsync() => UseCase.InvokeAsync();

    [Then("Result.MergeRequestStatesRetainLegacyMeaning should be true")]
    [Trait("UAC", "01"), Fact(DisplayName = "UAC01: the merge-request states should retain their meaning")]
    public void UAC01_MergeRequestStatesRetainLegacyMeaning() => Result.MergeRequestStatesRetainLegacyMeaning.ShouldBe(true, Result.Diagnostic("MergeRequestStatesRetainLegacyMeaning"));

    [Then("Result.MergeRequestActionsRespectState should be true")]
    [Trait("UAC", "02"), Fact(DisplayName = "UAC02: the merge-request actions should respect the state")]
    public void UAC02_MergeRequestActionsRespectState() => Result.MergeRequestActionsRespectState.ShouldBe(true, Result.Diagnostic("MergeRequestActionsRespectState"));

    [Then("Result.FeatureProposalExposesClientState should be true")]
    [Trait("UAC", "03"), Fact(DisplayName = "UAC03: the feature proposal should expose the client state")]
    public void UAC03_FeatureProposalExposesClientState() => Result.FeatureProposalExposesClientState.ShouldBe(true, Result.Diagnostic("FeatureProposalExposesClientState"));

    [Then("Result.PackageConsumerReadsGitFlowContract should be true")]
    [Trait("UAC", "04"), Fact(DisplayName = "UAC04: the package consumer should read the git-flow contract")]
    public void UAC04_PackageConsumerReadsGitFlowContract() => Result.PackageConsumerReadsGitFlowContract.ShouldBe(true, Result.Diagnostic("PackageConsumerReadsGitFlowContract"));
}
