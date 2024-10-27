namespace LowlandTech.Core.Types;

/// <summary>
/// Base class for a plugin.
/// </summary>
public abstract class Plugin : IPlugin
{
    /// <summary>
    /// Gets the identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the active state.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the company.
    /// </summary>
    public string? Company { get; }

    /// <summary>
    /// Gets the company URL.
    /// </summary>
    public string? Copyright { get; }

    /// <summary>
    /// Gets the URL.
    /// </summary>
    public string? Url { get; }

    /// <summary>
    /// Gets the version.
    /// </summary>
    public string? Version { get; }

    /// <summary>
    /// Gets the authors.
    /// </summary>
    public string? Authors { get; }

    /// <summary>
    /// Gets the assemblies.
    /// </summary>
    [JsonIgnore]
    public List<Assembly> Assemblies { get; } = new();

    /// <summary>
    /// Installs the plugin.
    /// </summary>
    /// <param name="services"></param>
    public abstract void Install(ServiceRegistry services);

    /// <summary>
    /// Configures the plugin.
    /// </summary>
    /// <param name="app"></param>
    public abstract void Configure(WebApplication app);
}
