namespace LowlandTech.Plugin1.Domain.UseCases;

public class Plugin1UseCase(Plugin1Context context) : IUseCase
{
    public async Task SeedAsync()
    {
        // Seed sample data
        context.Entities.Add(new Plugin1Entity { Name = "Test Entity" });
        await context.SaveChangesAsync();
    }
}