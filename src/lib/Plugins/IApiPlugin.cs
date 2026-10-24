
namespace LowlandTech.Core.Plugins;

/// <summary>
/// The single entry point a plugin assembly exposes to the host.
/// SPEC2050 US01.
/// </summary>
public interface IApiPlugin
{
    /// <summary>
    /// Identity, version, dependencies, and bootstrap flag.
    /// </summary>
    PluginDescriptor Descriptor { get; }

    /// <summary>
    /// Seed steps that run after <see cref="InstallAsync"/>.
    /// SPEC2050 US15.
    /// </summary>
    IReadOnlyList<IPluginSeed> Seeds => [];

    /// <summary>
    /// Feature packages the plugin contributes. The host calls
    /// <see cref="IFeaturePackage.Register"/> on each during
    /// <c>MakeAvailableAsync</c> so feature descriptors, endpoint groups,
    /// background jobs, and MCP tools land in their respective registries.
    /// SPEC2050 US03 / US10 / US11.
    /// </summary>
    IReadOnlyList<IFeaturePackage> Features => [];

    /// <summary>
    /// Register services into the host's <see cref="IServiceCollection"/>.
    /// Runs once per host startup (bootstrap plugins) or once per plugin load (hot plugins).
    /// </summary>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>
    /// Optional: contribute ASP.NET Core authentication schemes.
    /// SPEC2050 US12. Bootstrap plugins only.
    /// </summary>
    void ConfigureAuthentication(IAuthenticationSchemeRegistry registry, IConfiguration configuration) { }

    /// <summary>
    /// Optional: contribute ASP.NET Core authorization policies.
    /// SPEC2050 US13. Bootstrap plugins only.
    /// </summary>
    void ConfigureAuthorization(IAuthorizationPolicyRegistry registry) { }

    /// <summary>
    /// Optional: contribute RBAC permission keys and an i18n resource namespace.
    /// The host RBAC admin UI enumerates <see cref="IPermissionRegistry"/> entries
    /// for role assignment; the i18n resolver consults <see cref="IResourceRegistry"/>
    /// namespaces to localize plugin-owned strings.
    /// SPEC2050 US09.
    /// </summary>
    void ConfigurePermissions(IPermissionRegistry permissions, IResourceRegistry resources) { }

    /// <summary>
    /// Wire the plugin into a running host. The plugin is responsible for
    /// EVERY piece of wiring it needs (middleware ordering, route mapping,
    /// background services, event hooks) — it MUST NOT assume the host has
    /// already configured any of its dependencies.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <paramref name="host"/> parameter is typed as <see cref="object"/>
    /// so the core contract stays environment-agnostic. The same plugin can
    /// run inside an ASP.NET Core <c>WebApplication</c>, a MAUI
    /// <c>MauiAppBuilder</c>, a Photino native shell, or an Electron host —
    /// the plugin pattern-matches on the runtime type and does the appropriate
    /// wiring for that environment, throwing <see cref="NotSupportedException"/>
    /// for host types it doesn't know.
    /// </para>
    /// <para>
    /// This is required (no default no-op) because every plugin needs to make
    /// a deliberate decision about how it integrates with each host type it
    /// supports — silently doing nothing tends to hide misconfiguration.
    /// </para>
    /// SPEC2050.
    /// </remarks>
    void Use(object host);

    /// <summary>
    /// Run schema migrations and any first-install setup.
    /// SPEC2050 US01.
    /// </summary>
    Task InstallAsync(IPluginInstallContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Reverse migrations and drop the plugin schema.
    /// SPEC2050 US01.
    /// </summary>
    Task UninstallAsync(IPluginInstallContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Quiesce the plugin before a hot-reload swap.
    /// Default implementation is a no-op so existing plugins remain source-compatible
    /// until the host lifecycle fully wires suspend/resume callbacks.
    /// </summary>
    Task SuspendAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Resume the plugin after a hot-reload swap.
    /// Default implementation is a no-op so existing plugins remain source-compatible
    /// until the host lifecycle fully wires suspend/resume callbacks.
    /// </summary>
    Task ResumeAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
