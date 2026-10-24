namespace LowlandTech.Core;

/// <summary>
/// Represents the currently authenticated user.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// The unique identifier of the current user.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// The display name of the current user.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The email address of the current user.
    /// </summary>
    string Email { get; }

    /// <summary>
    /// The initials to display for the user avatar.
    /// </summary>
    string Initials { get; }
}
