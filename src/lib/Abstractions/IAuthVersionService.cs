namespace LowlandTech.Core.Abstractions;

/// <summary>
/// Service for managing the authentication version stamp.
/// Used to invalidate all existing sessions when the database is reset.
/// </summary>
public interface IAuthVersionService
{
    /// <summary>
    /// Gets the current auth version. Changes when DB is reset.
    /// </summary>
    string GetCurrentVersion();

    /// <summary>
    /// Rotates the auth version, invalidating all existing sessions.
    /// Called when database is detected to be reset.
    /// </summary>
    void RotateVersion();

    /// <summary>
    /// Validates that the given version matches the current version.
    /// </summary>
    bool IsValidVersion(string? version);
}
