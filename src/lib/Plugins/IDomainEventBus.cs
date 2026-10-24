namespace LowlandTech.Core.Plugins;

/// <summary>
/// Single call site for every <c>@postevent:</c> tag in the Agentic Gherkin specs.
/// Plugins publish typed events; subscribers (registered via <see cref="IEventRouter"/>)
/// observe them. Tests assert via <c>eventBus.Captured&lt;TEvent&gt;()</c>.
/// SPEC2050 — template for every spec that declares <c>@postevent:</c>.
/// </summary>
public interface IDomainEventBus
{
    Task EmitAsync<TEvent>(TEvent payload, CancellationToken ct = default)
        where TEvent : notnull;
}
