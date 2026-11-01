namespace LowlandTech.Core.Contracts.Sessions;

/// <summary>
/// The targets a session content descriptor may name.
/// <para>
/// The vocabulary is shared, the decision is not. The host decides whether a
/// named target is admitted: a descriptor that names the outer shell or the
/// root router is refused by the host, because those belong to the consuming
/// host and not to the content that it serves.
/// </para>
/// </summary>
public static class SessionContentTargets
{
    /// <summary>The bounded content root of a session.</summary>
    public const string SessionContent = "session-content";

    /// <summary>The outer application shell, which the consuming host owns.</summary>
    public const string OuterShell = "outer-shell";

    /// <summary>The root router of the application, which the consuming host owns.</summary>
    public const string RootRouter = "root-router";

    /// <summary>An explicit contribution slot that the host declares.</summary>
    public const string ExplicitHostSlot = "explicit-host-slot";
}

/// <summary>
/// One optional contribution the provider offers to the shell.
/// </summary>
/// <param name="Kind">The kind of the contribution, for example a navigation entry.</param>
/// <param name="Slot">The explicit host slot the contribution belongs to.</param>
public sealed record SessionContentIntent(string Kind, string Slot);

/// <summary>
/// The versioned content descriptor of a provider.
/// <para>
/// The descriptor is framework-neutral and package-neutral: it names the
/// session it belongs to, the version of the contract it speaks, the renderer
/// and the assets it carries, the content root it targets, the layout it
/// contributes and the pinned transitions it references. It defines no route
/// and no transition wire protocol of its own, and it carries no host type.
/// </para>
/// </summary>
public sealed record SessionContentDescriptor
{
    /// <summary>The version of the descriptor that a host admits.</summary>
    public const string SupportedVersion = "1.0.0";

    /// <summary>The version of the descriptor.</summary>
    public required string Version { get; init; }

    /// <summary>The session the content belongs to.</summary>
    public required Guid SessionId { get; init; }

    /// <summary>The context the content was resolved for, when it names one.</summary>
    public Guid? ContextId { get; init; }

    /// <summary>The renderer of the content.</summary>
    public required string Renderer { get; init; }

    /// <summary>The version of the renderer.</summary>
    public required string RendererVersion { get; init; }

    /// <summary>The identities of the assets the content carries.</summary>
    public IReadOnlyList<string> Assets { get; init; } = [];

    /// <summary>The target the content mounts into.</summary>
    public required string Target { get; init; }

    /// <summary>The layout the content contributes, by pinned field name.</summary>
    public IReadOnlyDictionary<string, string> Layout { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>The optional contributions the content offers to the shell.</summary>
    public IReadOnlyList<SessionContentIntent> Intents { get; init; } = [];

    /// <summary>The pinned transitions the content references.</summary>
    public IReadOnlyList<string> Transitions { get; init; } = [];
}

/// <summary>
/// The provider of the content of one session.
/// <para>
/// The provider is the one entry point of the contract: a content-owning
/// plugin registers it, and a host reads the descriptor of a session through
/// it. The provider answers with the descriptor or with nothing, and it never
/// answers with a host type.
/// </para>
/// </summary>
public interface ISessionContentProvider
{
    /// <summary>The content of the session, or null when the provider has none.</summary>
    /// <param name="sessionId">The session the content belongs to.</param>
    /// <param name="cancellationToken">The token of the read.</param>
    /// <returns>The descriptor of the content, or null.</returns>
    Task<SessionContentDescriptor?> GetAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
