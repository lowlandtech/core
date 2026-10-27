namespace LowlandTech.Core.Tests;

/// <summary>Discoverable acceptance inventory for CORE0002-US02-SC01.</summary>
public sealed class PluginContributionContractAcceptanceTests
{
    [Fact(Skip = "UAC01: portable target, contribution and definition declarations remain to be implemented")]
    public void UAC01_PortableDeclarationsContainNoRuntimeTypeNames() =>
        throw new NotImplementedException();

    [Fact(Skip = "UAC02: exact Plugins and Backend consumers and versioned manifest compatibility remain unproved")]
    public void UAC02_ConsumersUseOneVersionedContract() =>
        throw new NotImplementedException();

    [Fact(Skip = "UAC03: isolated package-only manifest and content-hash proof remains unimplemented")]
    public void UAC03_PackageOnlyConsumerValidatesDeclarations() =>
        throw new NotImplementedException();

    [Fact(Skip = "UAC04: contract dependency closure and trim analysis remain unproved")]
    public void UAC04_ContractHasNoHostOrEfDependencies() =>
        throw new NotImplementedException();
}
