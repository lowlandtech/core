namespace LowlandTech.Core.Tests.CORE0001.ContainerContract.US01.Contracts;

[Trait("CORE", "0001"), Trait("US", "01"), Trait("SC", "01")]
[UserStory("01", "The canonical container contract")]
public sealed class SC01_ContainerContractCarriesTheOptionalItemIdentity(global::LowlandTech.Core.Tests.Refinement.ScenarioFixture<UseCases.ContainerContractUseCase> fixture) : global::LowlandTech.Core.Tests.Refinement.UseCaseScenario<UseCases.ContainerContractUseCase>(fixture)
{
    [Given("the canonical container contract of the published producer")]
    protected override Task GivenAsync() => UseCase.ArrangeAsync();

    [When("the contract is read for the members that create a container item")]
    protected override Task WhenAsync() => UseCase.InvokeAsync();

    [Then("Result.CreateMembersDeclared should be true")]
    [Trait("UAC", "03"), Fact(DisplayName = "UAC03: the create members should be declared")]
    public void UAC03_CreateMembersDeclared() => Result.CreateMembersDeclared.ShouldBe(true, Result.Diagnostic("CreateMembersDeclared"));

    [Then("Result.ItemIdentityOptional should be true")]
    [Trait("UAC", "04"), Fact(DisplayName = "UAC04: the item identity should be optional")]
    public void UAC04_ItemIdentityOptional() => Result.ItemIdentityOptional.ShouldBe(true, Result.Diagnostic("ItemIdentityOptional"));

    [Then("Result.ItemIdentityIsLastMember should be true")]
    [Trait("UAC", "05"), Fact(DisplayName = "UAC05: the item identity should be the last member")]
    public void UAC05_ItemIdentityIsLastMember() => Result.ItemIdentityIsLastMember.ShouldBe(true, Result.Diagnostic("ItemIdentityIsLastMember"));
}
