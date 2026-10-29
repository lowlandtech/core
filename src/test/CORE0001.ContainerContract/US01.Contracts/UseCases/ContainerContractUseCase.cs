using System.Reflection;

namespace LowlandTech.Core.Tests.CORE0001.ContainerContract.US01.Contracts.UseCases;

/// <summary>
/// The Core owned half of UAC03 of CORE0001-US02-SC01: the canonical container
/// contract has to carry the member that the consumer cutover depends on.
///
/// A consumer that adopts the canonical package instead of a vendored copy
/// implements <c>LowlandTech.Domain.Abstractions.IContainerService</c>, so the
/// published interface and the implementors have to agree on the member list.
/// The optional item identity is that agreement: it is the one member the
/// canonical interface did not carry while the implementors already had it,
/// and its absence makes such an implementor fail to compile against the
/// package with a CS0535.
///
/// The observations are captured here so that every Fact of the scenario
/// asserts one of them against the production contract.
/// </summary>
public sealed class ContainerContractUseCase : global::LowlandTech.Core.Tests.Refinement.IUseCase
{
    /// <summary>The two members that create a container item.</summary>
    private static readonly string[] CreateMethods =
    {
        nameof(LowlandTech.Domain.Abstractions.IContainerService.CreateItemFromComponentAsync),
        nameof(LowlandTech.Domain.Abstractions.IContainerService.CreateItemFromDescriptorAsync)
    };

    private readonly List<global::LowlandTech.Core.Tests.Refinement.ResultCase> results = [];

    /// <summary>The named result cases of this scenario.</summary>
    public IReadOnlyList<global::LowlandTech.Core.Tests.Refinement.ResultCase> ResultCases => results;

    /// <summary>Both members that create a container item are declared.</summary>
    public bool? CreateMembersDeclared { get; private set; }

    /// <summary>Every create member carries the optional item identity.</summary>
    public bool? ItemIdentityOptional { get; private set; }

    /// <summary>The item identity is the last member of every create member.</summary>
    public bool? ItemIdentityIsLastMember { get; private set; }

    /// <summary>The diagnostic that the measurement of a named result case recorded.</summary>
    public string Diagnostic(string key)
        => results.FirstOrDefault(result => result.Key == key)?.Diagnostic ?? "no diagnostic was recorded";

    public Task ArrangeAsync() => Task.CompletedTask;

    public Task InvokeAsync()
    {
        var contract = typeof(LowlandTech.Domain.Abstractions.IContainerService);
        var methods = CreateMethods
            .Select(name => contract.GetMethod(name))
            .ToArray();

        Record("CreateMembersDeclared",
            methods.All(method => method is not null),
            $"The contract declares {methods.Count(method => method is not null)} of {CreateMethods.Length} create members.");

        var identities = methods
            .Where(method => method is not null)
            .Select(method => method!.GetParameters().SingleOrDefault(parameter => parameter.Name == "itemId"))
            .ToArray();
        Record("ItemIdentityOptional",
            identities.Length == CreateMethods.Length
            && identities.All(identity => identity is not null
                                          && identity!.ParameterType == typeof(Guid?)
                                          && identity.IsOptional
                                          && identity.HasDefaultValue
                                          && identity.DefaultValue is null),
            "Every create member carries an optional nullable item identity that defaults to null.");
        Record("ItemIdentityIsLastMember",
            methods.Where(method => method is not null).All(method =>
            {
                var parameters = method!.GetParameters();
                var identity = parameters.SingleOrDefault(parameter => parameter.Name == "itemId");
                return identity is not null && parameters[^1] == identity;
            }),
            "The item identity is the last member of every create member.");

        return Task.CompletedTask;
    }

    private void Record(string key, bool outcome, string diagnostic)
    {
        results.RemoveAll(result => result.Key == key);
        results.Add(new global::LowlandTech.Core.Tests.Refinement.ResultCase(
            key, outcome ? "Observed" : "NotObserved", diagnostic));
        switch (key)
        {
            case "CreateMembersDeclared": CreateMembersDeclared = outcome; break;
            case "ItemIdentityOptional": ItemIdentityOptional = outcome; break;
            case "ItemIdentityIsLastMember": ItemIdentityIsLastMember = outcome; break;
        }
    }
}
