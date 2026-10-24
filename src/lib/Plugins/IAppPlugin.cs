
namespace LowlandTech.Core.Plugins;

/// <summary>
/// Entry point a frontend plugin assembly exposes to the app host (Blazor Server,
/// MAUI shell, desktop, etc.). Mirrors <see cref="IApiPlugin"/> for the frontend
/// side: registers frontend services, Blazor component assemblies, Fluxor stores,
/// HTTP clients, and frontend-specific middleware.
/// </summary>
public interface IAppPlugin
{
    /// <summary>Identity and version metadata for this frontend plugin.</summary>
    PluginDescriptor Descriptor { get; }

    /// <summary>
    /// Register services into the app host's <see cref="IServiceCollection"/>.
    /// </summary>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>
    /// Wire the plugin into a running app host. The plugin pattern-matches on
    /// <paramref name="host"/> to support multiple host environments
    /// (ASP.NET Core <c>WebApplication</c>, MAUI <c>MauiAppBuilder</c>, etc.)
    /// and performs all wiring it needs — route assembly registration, middleware,
    /// event hooks — without assuming the host has done anything for it.
    /// </summary>
    void Use(object host);

    /// <summary>
    /// Stylesheet hrefs contributed by this plugin to the host HTML shell.
    /// Each value should be a root-relative href, typically of the form
    /// <c>_content/{AssemblyName}/css/styles.css</c>.
    /// The host renders these as <c>&lt;link rel="stylesheet"&gt;</c> tags inside
    /// <c>&lt;head&gt;</c> before the page body.
    /// </summary>
    IReadOnlyList<string> StylesheetLinks => [];

    /// <summary>
    /// Script src hrefs contributed by this plugin to the host HTML shell.
    /// Each value should be a root-relative href, typically of the form
    /// <c>_content/{AssemblyName}/js/plugin.js</c>.
    /// The host renders these as <c>&lt;script&gt;</c> tags at the end of
    /// <c>&lt;body&gt;</c>, before the framework script.
    /// </summary>
    IReadOnlyList<string> ScriptLinks => [];

    /// <summary>
    /// Inline JavaScript blocks contributed by this plugin to the host HTML shell.
    /// Each value is raw JavaScript source that will be wrapped in a <c>&lt;script&gt;</c>
    /// tag and injected at the end of <c>&lt;body&gt;</c>, after external script links
    /// and before the framework script.
    /// </summary>
    IReadOnlyList<string> InlineScripts => [];
}
