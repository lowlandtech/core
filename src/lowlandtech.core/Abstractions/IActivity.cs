namespace LowlandTech.Core.Abstractions;

/// <summary>
/// Contract for an activity.
/// </summary>
public interface IActivity
{
    /// <summary>
    /// Execute the activity.
    /// </summary>
    /// <returns></returns>
    Task ExecuteAsync();
}
