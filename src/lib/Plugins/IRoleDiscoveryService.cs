namespace LowlandTech.Core.Plugins;

/// <summary>
/// Service that synchronizes role permission claims in the database with the
/// discovered set from all installed plugins.
/// </summary>
public interface IRoleDiscoveryService
{
    /// <summary>
    /// Ensures that every role in <paramref name="realmId"/> has the permission
    /// keys discovered by <see cref="IRolePrivilegeDiscovery"/>.
    /// Idempotent: safe to call multiple times.
    /// </summary>
    Task SyncAsync(string realmId = "master", CancellationToken cancellationToken = default);
}
