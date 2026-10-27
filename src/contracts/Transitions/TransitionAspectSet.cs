using System.Text.Json;
using System.Text.Json.Serialization;

namespace LowlandTech.Core.Contracts.Transitions;

/// <summary>
/// The aspects of a transition document. The member order is the wire order of
/// the aspects object.
/// </summary>
public sealed record TransitionAspectSet(
    TransitionUseCase UseCase,
    IReadOnlyList<ComponentIntent>? Intents = null,
    string? Intent = null,
    IReadOnlyDictionary<string, TransitionPrecondition>? Inputs = null,
    ClientLayout? Layout = null,
    ClientContainer? Container = null,
    string Authentication = "required",
    IReadOnlyDictionary<string, TransitionTrigger>? Triggers = null,
    TransitionStateMachine? StateMachine = null);

/// <summary>The trigger pair of a transition. The keys are the phase names.</summary>
public sealed record TransitionTrigger(string Effect);

/// <summary>
/// The acceptance of a transition: its intent and its result cases.
/// </summary>
public sealed record TransitionUseCase(string Intent, IReadOnlyList<TransitionCase> Cases);

/// <summary>
/// One result case of a transition. The input and the given state are carried
/// as data, so a case never references a host runtime type.
/// </summary>
public sealed record TransitionCase(
    string Key,
    string Outcome,
    string? Diagnostic,
    JsonElement? Input,
    IReadOnlyList<TransitionSetup>? Given,
    bool Cancelled = false,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] JsonElement? Expected = default);

/// <summary>The setup of one precondition of a result case.</summary>
public sealed record TransitionSetup(string Capability, JsonElement? Input);
