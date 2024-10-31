namespace LowlandTech.Plugin1.Domain.Triggers;

public class Plugin1EntitySaved : IAfterSaveTrigger<Plugin1Entity>
{
    public Task AfterSave(ITriggerContext<Plugin1Entity> context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}