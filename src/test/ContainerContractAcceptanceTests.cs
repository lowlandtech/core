namespace LowlandTech.Core.Tests;

/// <summary>
/// The Core owned half of UAC03 of CORE0001-US02-SC01: the canonical container
/// contract has to carry the member that the consumer cutover depends on.
///
/// A consumer that adopts the canonical package instead of a vendored copy
/// implements <c>LowlandTech.Domain.Abstractions.IContainerService</c>, so the
/// published interface and the implementor have to agree on the member list.
/// The optional item identity is that agreement: it is the one member the
/// canonical interface did not carry while the implementors already had it, and
/// its absence makes such an implementor fail to compile against the package
/// with a CS0535.
/// </summary>
public sealed class ContainerContractAcceptanceTests
{
    /// <summary>The two members that create a container item.</summary>
    private static readonly string[] CreateMethods =
    {
        nameof(LowlandTech.Domain.Abstractions.IContainerService.CreateItemFromComponentAsync),
        nameof(LowlandTech.Domain.Abstractions.IContainerService.CreateItemFromDescriptorAsync)
    };

    [Fact]
    public void UAC03_TheCanonicalContainerContractCarriesTheOptionalItemIdentity()
    {
        var contract = typeof(LowlandTech.Domain.Abstractions.IContainerService);

        foreach (var name in CreateMethods)
        {
            var method = contract.GetMethod(name);
            method.ShouldNotBeNull($"The canonical container contract does not declare '{name}'.");

            var parameters = method!.GetParameters();
            var identity = parameters.SingleOrDefault(parameter => parameter.Name == "itemId");
            identity.ShouldNotBeNull($"'{name}' does not carry the optional item identity.");

            identity!.ParameterType.ShouldBe(typeof(Guid?));
            identity.IsOptional.ShouldBeTrue($"The item identity of '{name}' is not optional.");
            identity.HasDefaultValue.ShouldBeTrue($"The item identity of '{name}' has no default value.");
            identity.DefaultValue.ShouldBeNull($"The item identity of '{name}' does not default to null.");

            // The identity is the last member, so a caller that does not know it
            // keeps compiling against the canonical package.
            parameters[^1].ShouldBe(identity);
        }
    }
}
