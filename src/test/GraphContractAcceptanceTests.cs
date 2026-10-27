namespace LowlandTech.Core.Tests;

/// <summary>Coordinator-owned inventory for CORE0002-US01-SC01; no behavior is accepted by skipped Facts.</summary>
public sealed class GraphContractAcceptanceTests
{
    [Fact(Skip = "UAC01: publish the gs.transition/v1 document contract and compare Backend serialized round trips.")]
    public void UAC01_BackendTransitionDocumentRoundTrips()
        => throw new NotImplementedException("Capture exact Backend definition fixtures through the new contract package.");

    [Fact(Skip = "UAC02: build exact package-only Backend and external plugin consumers with one contract assembly.")]
    public void UAC02_ConsumersUseOneContractPackage()
        => throw new NotImplementedException("Capture exact package pins and package-only consumer results.");

    [Fact(Skip = "UAC03: validate owner, schema, scope, capability and hash refusals before publication.")]
    public void UAC03_InvalidDefinitionsReceiveNamedRefusals()
        => throw new NotImplementedException("Observe named parser/validator refusals from real contract inputs.");

    [Fact(Skip = "UAC04: inspect new contract package closure for zero EF, ASP.NET and host runtime dependencies.")]
    public void UAC04_PortablePackageHasNoHostDependencies()
        => throw new NotImplementedException("Inspect the generated nupkg and its dependency graph.");

    [Fact(Skip = "UAC05: preserve the existing LowlandTech.Core compatibility package and pinned consumers.")]
    public void UAC05_ExistingCorePackageRemainsAvailable()
        => throw new NotImplementedException("Run both package-only consumers with exact immutable versions.");
}
