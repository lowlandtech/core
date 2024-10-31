namespace LowlandTech.Plugin1.Domain.Triggers;

public class Plugin1EntitySaving : IBeforeSaveTrigger<Plugin1Entity>
{
    public Task BeforeSave(ITriggerContext<Plugin1Entity> context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}