namespace LowlandTech.Core.Abstractions;

/// <summary>
/// Contract for a plugin.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Gets the identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the active state.
    /// </summary>
    bool IsActive { get; set; }

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
    public List<Assembly>? Assemblies { get; }

    /// <summary>
    /// Installs the plugin.
    /// </summary>
    /// <param name="services"></param>
    void Install(ServiceRegistry services);

    /// <summary>
    /// Configures the plugin.
    /// </summary>
    /// <param name="container">The ioc container.</param>
    Task Configure(IContainer container);
}
