namespace LowlandTech.Core.Domain.Types;

/// <summary>
/// Gets the provider options.
/// </summary>
public class ProviderOptions
{
    /// <summary>
    /// The provider context.
    /// </summary>
    public const string ProviderContext = "ProviderContext";

    /// <summary>
    /// Gets or sets the provider.
    /// </summary>
    public Providers Provider { get; set; } = Providers.InMemory;

    /// <summary>
    /// Gets or sets to use lazy loading.
    /// </summary>
    public bool UseProxies { get; set; }

    /// <summary>
    /// Gets or sets to use sensitive data logging.
    /// </summary>
    public bool UseSensitiveDataLogging { get; set; }

    /// <summary>
    /// gets or sets to use ef core triggers.
    /// </summary>
    public bool UseTriggers { get; set; }

    /// <summary>
    /// Gets or sets the domain prefix.
    /// </summary>
    public string Prefix { get; set; } = "LowlandTech.Domain";
}