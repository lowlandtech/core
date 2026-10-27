namespace LowlandTech.Core.Tests;

/// <summary>Acceptance inventory for CORE0005-US01-SC01.</summary>
public sealed class GitFlowClientContractAcceptanceTests
{
    [Fact(Skip = "UAC01: versioned merge-request states and legacy wire compatibility remain unimplemented")]
    public void UAC01_MergeRequestStatesRetainLegacyMeaning() => throw new NotImplementedException();

    [Fact(Skip = "UAC02: source-owned reviewing and terminal transitions remain unimplemented")]
    public void UAC02_MergeRequestActionsRespectState() => throw new NotImplementedException();

    [Fact(Skip = "UAC03: proposal title, lifecycle and Gherkin specification remain unimplemented")]
    public void UAC03_FeatureProposalExposesClientState() => throw new NotImplementedException();

    [Fact(Skip = "UAC04: immutable package-only Core and Coder consumer proof remain open")]
    public void UAC04_PackageConsumerReadsGitFlowContract() => throw new NotImplementedException();
}
