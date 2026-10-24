namespace LowlandTech.Core.Plugins;

/// <summary>
/// Tracks authentication schemes contributed by plugins.
/// SPEC2050 US12.
/// </summary>
public interface IAuthenticationSchemeRegistry
{
    IReadOnlyCollection<AuthenticationSchemeDescriptor> All { get; }

    /// <summary>
    /// Register a scheme.
    /// </summary>
    /// <exception cref="AuthenticationSchemeConflictException">A different plugin already registered the same scheme name.</exception>
    void Register(AuthenticationSchemeDescriptor descriptor);

    bool IsRegistered(string schemeName);
}
