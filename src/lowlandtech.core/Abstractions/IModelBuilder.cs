namespace LowlandTech.Core.Abstractions;

/// <summary>
/// Contract for a model builder.
/// </summary>
public interface IModelBuilder
{
    /// <summary>
    /// Build the model.
    /// </summary>
    /// <param name="modelBuilder"></param>
    public void Build(ModelBuilder modelBuilder);
}
