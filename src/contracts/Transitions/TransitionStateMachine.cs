using System.Text.Json;

namespace LowlandTech.Core.Contracts.Transitions;

/// <summary>
/// The authored state machine of a transition. The machine travels as data; the
/// runtime that executes it stays with the host.
/// </summary>
public sealed record TransitionStateMachine(
    string Initial,
    IReadOnlyList<string> States,
    IReadOnlyList<TransitionStateAction> Transitions);

/// <summary>One action of a state machine.</summary>
public sealed record TransitionStateAction(
    string Key,
    string From,
    string To,
    TransitionInputSchema InputSchema,
    IReadOnlyList<TransitionCondition> Preconditions,
    IReadOnlyList<JsonElement> Steps,
    IReadOnlyList<TransitionCondition> Postconditions);

/// <summary>The input schema of a state machine action.</summary>
public sealed record TransitionInputSchema(
    string Type,
    bool AdditionalProperties,
    IReadOnlyDictionary<string, TransitionPrecondition> Properties,
    IReadOnlyList<string> Required);

/// <summary>One condition of a state machine action.</summary>
public sealed record TransitionCondition(string Key, JsonElement? Expression);
