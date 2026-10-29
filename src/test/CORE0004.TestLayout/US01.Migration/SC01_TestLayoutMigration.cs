namespace LowlandTech.Core.Tests.CORE0004.TestLayout.US01.Migration;

[Trait("CORE", "0004"), Trait("US", "01"), Trait("SC", "01")]
[UserStory("01", "TestLayoutMigration")]
public sealed class SC01_TestLayoutMigration(global::LowlandTech.Core.Tests.Refinement.ScenarioFixture<UseCases.TestLayoutMigrationUseCase> fixture) : global::LowlandTech.Core.Tests.Refinement.UseCaseScenario<UseCases.TestLayoutMigrationUseCase>(fixture)
{
    [Given("the source-owned fixtures and current accepted package or host checkpoint")]
    protected override Task GivenAsync() => UseCase.ArrangeAsync();

    [When("the TestLayoutMigration use case observes its declared success and refusal boundaries")]
    protected override Task WhenAsync() => UseCase.InvokeAsync();

    [Then("Result.LegacyCoverageMapped should be true")]
    [Trait("UAC", "01"), Fact(DisplayName = "UAC01: captured result should be true", Skip = "TODO: CORE0004-US01-SC01: implement map every current Fact/Theory including producer/closure/git-flow/container and five Backend parity fixtures to unchanged source criteria.")]
    public void UAC01_LegacyCoverageMapped() => Result.LegacyCoverageMapped.ShouldBe(true);

    [Then("Result.ScenarioShapeProven should be true")]
    [Trait("UAC", "02"), Fact(DisplayName = "UAC02: captured result should be true", Skip = "TODO: CORE0004-US01-SC01: implement actual Core scenario scan with one Given/When and expression-bodied uniquely tagged Facts, named result cases and fixture-owned one-time invocation.")]
    public void UAC02_ScenarioShapeProven() => Result.ScenarioShapeProven.ShouldBe(true);

    [Then("Result.InvalidLayoutRejected should be true")]
    [Trait("UAC", "03"), Fact(DisplayName = "UAC03: captured result should be true", Skip = "TODO: CORE0004-US01-SC01: implement negative fixtures for missing/duplicate UAC, wrong paths, repeated steps, multi-assertion, empty selection and unfinished skips.")]
    public void UAC03_InvalidLayoutRejected() => Result.InvalidLayoutRejected.ShouldBe(true);

    [Then("Result.ExecutedBehaviorRetained should be true")]
    [Trait("UAC", "04"), Fact(DisplayName = "UAC04: captured result should be true", Skip = "TODO: CORE0004-US01-SC01: implement retained old suite until production-backed new equivalents execute; compare exact cases and fixture hashes.")]
    public void UAC04_ExecutedBehaviorRetained() => Result.ExecutedBehaviorRetained.ShouldBe(true);

    [Then("Result.TrimmedConsumerRetained should be true")]
    [Trait("UAC", "05"), Fact(DisplayName = "UAC05: captured result should be true", Skip = "TODO: CORE0004-US01-SC01: implement native release build, tests and nonpublishing package preflight with exact pin and zero owned trim warnings.")]
    public void UAC05_TrimmedConsumerRetained() => Result.TrimmedConsumerRetained.ShouldBe(true);
}
