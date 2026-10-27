using System.Security.Cryptography;
using System.Text;

namespace LowlandTech.Core.Contracts.Plugins;

/// <summary>The identity of a plugin: its publisher and its key.</summary>
public sealed record PluginKey(string Publisher, string Key);

/// <summary>
/// The algorithm qualified content identity of a plugin artifact. The byte
/// framing of an artifact belongs to artifact verification, not to this value.
/// </summary>
public sealed record PluginContentHash(string Algorithm, string Digest)
{
    /// <summary>The hash algorithms the contract supports.</summary>
    public static IReadOnlyList<string> SupportedAlgorithms { get; } = ["sha256", "sha512"];

    /// <summary>Whether the contract can compute this algorithm.</summary>
    public static bool IsSupported(string algorithm)
        => SupportedAlgorithms.Contains(algorithm, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Computes the content hash of the given content. The digest is lower case
    /// hexadecimal so two producers of the same content agree on it.
    /// </summary>
    public static PluginContentHash Compute(string algorithm, ReadOnlySpan<byte> content)
    {
        if (!IsSupported(algorithm))
            throw new ArgumentException($"The content hash algorithm '{algorithm}' is not supported.", nameof(algorithm));
        var normalized = algorithm.ToLowerInvariant();
        var digest = normalized == "sha256"
            ? SHA256.HashData(content)
            : SHA512.HashData(content);
        return new PluginContentHash(normalized, Convert.ToHexString(digest).ToLowerInvariant());
    }

    /// <summary>Computes the content hash of the UTF-8 bytes of the given text.</summary>
    public static PluginContentHash Compute(string algorithm, string content)
        => Compute(algorithm, Encoding.UTF8.GetBytes(content));
}

/// <summary>The revision of a plugin: its identity and its content hashes.</summary>
public sealed record PluginRevision(PluginKey Key, PluginContentHash Definition, PluginContentHash Artifact);

/// <summary>
/// The scope of a plugin. A null future cursor denotes reality; a future
/// inherits the other scope coordinates.
/// </summary>
public sealed record PluginScope(string Host, string? Tenant, string? Client, string? Session, string? FutureCursor)
{
    /// <summary>Whether the scope names a future rather than reality.</summary>
    public bool IsFuture => FutureCursor is not null;

    /// <summary>Whether the scope names a host.</summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(Host);
}

/// <summary>The kind of the implementation a plugin binds to.</summary>
public enum PluginImplementationKind
{
    /// <summary>A native implementation of the host.</summary>
    Native,

    /// <summary>A data-defined implementation of the host.</summary>
    DefinitionBacked,

    /// <summary>An aspect implementation of the host.</summary>
    Aspect
}

/// <summary>The lifetime of an activated plugin.</summary>
public enum PluginLifetime
{
    /// <summary>One instance for the whole host.</summary>
    Shared,

    /// <summary>One instance per scope.</summary>
    Scoped
}

/// <summary>
/// The binding of a plugin to an implementation. The binding carries portable
/// keys, never a runtime type, a delegate or a scoped instance.
/// </summary>
public sealed record PluginImplementationBinding(PluginImplementationKind Kind, string ContractKey, string ImplementationKey);

/// <summary>The provenance of a plugin.</summary>
public sealed record PluginProvenance(string SourceKey, string Origin);

/// <summary>
/// The portable identity of a plugin: the revision, the contract version, the
/// environments, the dependencies, the binding, the lifetime and the
/// provenance. The availability and the activation of a plugin are separate
/// state and stay with the host.
/// </summary>
public sealed record PluginIdentity(
    PluginRevision Revision,
    string AbiVersion,
    IReadOnlyList<string> Environments,
    IReadOnlyList<PluginRevision> Dependencies,
    PluginImplementationBinding Binding,
    PluginLifetime Lifetime,
    IReadOnlyList<PluginProvenance> Provenance);
