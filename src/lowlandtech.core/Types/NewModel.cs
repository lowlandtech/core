namespace LowlandTech.Core.Types;

/// <summary>
/// Represents a new model.
/// </summary>
public class NewModel
{
    /// <summary>
    /// Gets/sets the name of the new model.
    /// </summary>
    [StringLength(50)]
    public string? Name { get; set; }
}
