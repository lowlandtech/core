namespace LowlandTech.Core.Contracts.Transitions;

/// <summary>
/// A capability reference of a host. A capability is a reference, never an
/// implementation: a data-defined plugin binds to a capability that is already
/// registered and never registers one.
/// </summary>
public sealed record CapabilityReference(string Key)
{
    /// <summary>
    /// The capabilities every host of the platform registers. The contract
    /// grants none of them implicitly: a host that admits a document that
    /// declares one of them registers it.
    /// </summary>
    public static IReadOnlyList<string> FixedCapabilities { get; } =
    [
        "client.access",
        "client.dummy-step",
        "client.context-reference",
        "component.access",
        "component.list-intents"
    ];

    /// <summary>
    /// Whether the capability is a kernel capability. A kernel capability owns
    /// the default input schema and the source identifier rule; every other
    /// capability owns no input schema.
    /// </summary>
    public bool IsKernel => Key.StartsWith("auth.", StringComparison.Ordinal)
        || Key.StartsWith("client.", StringComparison.Ordinal)
        || Key.StartsWith("component.", StringComparison.Ordinal);

    /// <summary>Whether the capability is one of the fixed platform capabilities.</summary>
    public bool IsFixed => FixedCapabilities.Contains(Key, StringComparer.Ordinal);

    /// <summary>
    /// The effect the start trigger of a transition with this capability carries.
    /// </summary>
    public string StartEffect => Key == "component.list-intents"
        ? "component.list"
        : Key is "client.access" or "client.dummy-step" or "component.access" or "client.context-reference"
            ? "graph.walk"
            : "server";

    /// <summary>The effect the end trigger of a transition always carries.</summary>
    public const string EndEffect = "graph.persist";

    /// <summary>Reads a capability reference from its dotted key.</summary>
    public static CapabilityReference Parse(string key) => new(key);
}
