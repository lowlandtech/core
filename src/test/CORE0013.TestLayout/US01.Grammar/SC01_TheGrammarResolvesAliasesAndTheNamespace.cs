namespace LowlandTech.Core.Tests.CORE0013.TestLayout.US01.Grammar;

[Trait("CORE", "0013"), Trait("US", "01"), Trait("SC", "01")]
[UserStory("01", "GrammarChecks")]
public sealed class SC01_TheGrammarResolvesAliasesAndTheNamespace(global::LowlandTech.Core.Tests.Refinement.ScenarioFixture<UseCases.GrammarChecksUseCase> fixture) : global::LowlandTech.Core.Tests.Refinement.UseCaseScenario<UseCases.GrammarChecksUseCase>(fixture)
{
    [Given("the Core owned layout gate and its alias, namespace and identity fixtures")]
    protected override Task GivenAsync() => UseCase.ArrangeAsync();

    [When("the grammar checks observe the alias form, the repeated UAC, the namespace and the path identity")]
    protected override Task WhenAsync() => UseCase.InvokeAsync();

    [Then("Result.AliasResolved should be true")]
    [Trait("UAC", "01"), Fact(DisplayName = "UAC01: the alias trait resolves like the literal")]
    public void UAC01_AliasResolved() => Result.AliasResolved.ShouldBe(true, Result.Diagnostic("AliasResolved"));

    [Then("Result.DuplicateRejected should be true")]
    [Trait("UAC", "02"), Fact(DisplayName = "UAC02: the repeated alias UAC is rejected")]
    public void UAC02_DuplicateRejected() => Result.DuplicateRejected.ShouldBe(true, Result.Diagnostic("DuplicateRejected"));

    [Then("Result.NamespaceRequired should be true")]
    [Trait("UAC", "03"), Fact(DisplayName = "UAC03: a scenario without a namespace is rejected")]
    public void UAC03_NamespaceRequired() => Result.NamespaceRequired.ShouldBe(true, Result.Diagnostic("NamespaceRequired"));

    [Then("Result.NamespaceIdentityChecked should be true")]
    [Trait("UAC", "04"), Fact(DisplayName = "UAC04: a namespace of another area is rejected")]
    public void UAC04_NamespaceIdentityChecked() => Result.NamespaceIdentityChecked.ShouldBe(true, Result.Diagnostic("NamespaceIdentityChecked"));

    [Then("Result.UnresolvedKeyReported should be true")]
    [Trait("UAC", "05"), Fact(DisplayName = "UAC05: a trait key with no constant is reported")]
    public void UAC05_UnresolvedKeyReported() => Result.UnresolvedKeyReported.ShouldBe(true, Result.Diagnostic("UnresolvedKeyReported"));

    [Then("Result.CoreTreeClean should be true")]
    [Trait("UAC", "06"), Fact(DisplayName = "UAC06: the Core tree keeps no violation")]
    public void UAC06_CoreTreeClean() => Result.CoreTreeClean.ShouldBe(true, Result.Diagnostic("CoreTreeClean"));
}
