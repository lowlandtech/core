namespace LowlandTech.Core.Plugins.HotReload;

/// <summary>
/// Configuration for <see cref="IHotReloadService"/>.
/// SPEC2050 US05.
/// </summary>
public sealed record HotReloadConfig
{
    /// <summary>How often the service polls the feed for new versions. Default: 300 seconds.</summary>
    public int PollingIntervalSeconds { get; init; } = 300;

    /// <summary>Webhook endpoint path to receive push notifications from the feed. Default: "/api/plugins/webhook".</summary>
    public string WebhookPath { get; init; } = "/api/plugins/webhook";

    /// <summary>
    /// When <c>true</c>, pre-release versions (e.g. "1.1.0-beta.1") trigger auto-update.
    /// Default: <c>false</c> — stable releases only.
    /// SPEC2050 US05 SC10 / SC11.
    /// </summary>
    public bool AllowPrerelease { get; init; } = false;

    /// <summary>
    /// When <c>true</c>, downgrade requests are accepted.
    /// Default: <c>false</c> — upgrades only.
    /// SPEC2050 US05 SC12.
    /// </summary>
    public bool AllowDowngrade { get; init; } = false;

    /// <summary>Root directory under which staging subdirectories are created.</summary>
    public string StagingRoot { get; init; } = "/staging";
}
