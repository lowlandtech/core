using System.Text.Json;
using LowlandTech.Core.Contracts.Plugins;

namespace LowlandTech.Core.Contracts.Transitions;

/// <summary>
/// What a host knows about a transition document when it admits it. The
/// expectation of the owner and of the scope, the registered capabilities and
/// the declared content hash come from the host; the document comes from the
/// plugin.
/// </summary>
public sealed record TransitionAdmissionRequest(
    TransitionDefinitionDocument Document,
    string ExpectedOwner,
    PortablePluginScope? Scope = null,
    PortablePluginScope? ExpectedScope = null,
    IReadOnlyCollection<string>? KnownCapabilities = null,
    PortablePluginContentHash? DeclaredContentHash = null);

/// <summary>
/// Validates a transition document and names every refusal of an inadmissible
/// document. The refusals keep the names of the host: wrong owner, wrong
/// schema, wrong scope, wrong capability reference, wrong content hash,
/// conflicting identity and the invalid aspects.
/// </summary>
public static class TransitionContractValidator
{
    /// <summary>
    /// The refusals of the document itself. The rules are the rules the host
    /// applies when it loads a document, so a document that passes here loads
    /// in the host.
    /// </summary>
    public static IReadOnlyList<TransitionContractDiagnostic> Validate(TransitionDefinitionDocument? document)
    {
        var diagnostics = new List<TransitionContractDiagnostic>();
        if (document is null)
        {
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.DocumentEmpty,
                "The transition document is empty.",
                "document"));
            return diagnostics;
        }

        if (document.Schema != TransitionContractDocument.SupportedSchema)
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.SchemaUnsupported,
                $"The transition schema '{document.Schema}' is not supported. Expected '{TransitionContractDocument.SupportedSchema}'.",
                "schema"));

        if (document.Kind != TransitionContractDocument.SupportedKind)
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.KindUnsupported,
                $"The transition kind '{document.Kind}' is not supported. Expected '{TransitionContractDocument.SupportedKind}'.",
                "kind"));

        if (string.IsNullOrWhiteSpace(document.Plugin))
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.OwnerMissing,
                "The transition owner is missing.",
                "plugin"));

        if (string.IsNullOrWhiteSpace(document.Key))
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.IdentityMissing,
                "The transition key is missing.",
                "key"));

        if (string.IsNullOrWhiteSpace(document.Source))
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.FieldMissing,
                "The transition source is missing.",
                "source"));

        if (string.IsNullOrWhiteSpace(document.EdgeType))
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.FieldMissing,
                "The transition edge type is missing.",
                "edgeType"));

        if (string.IsNullOrWhiteSpace(document.Target))
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.FieldMissing,
                "The transition target is missing.",
                "target"));

        if (string.IsNullOrWhiteSpace(document.Capability))
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.CapabilityUnknown,
                "The transition capability is missing.",
                "capability"));

        var aspects = document.Aspects;
        if (aspects is null)
        {
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.AspectsMissing,
                "The transition aspects are missing.",
                "aspects"));
            return diagnostics;
        }

        ValidateUseCase(aspects.UseCase, diagnostics);
        ValidateAuthentication(aspects.Authentication, diagnostics);
        ValidateIntent(aspects.Intent, diagnostics);
        ValidateInputs(aspects.Inputs, diagnostics);
        ValidateLayout(aspects.Layout, diagnostics);
        ValidateContainer(aspects.Container, diagnostics);
        ValidateTriggers(aspects.Triggers, diagnostics);
        ValidateStateMachine(aspects.StateMachine, diagnostics);
        return diagnostics;
    }

    /// <summary>
    /// The refusals of the admission of a document into a host: the refusals of
    /// the document itself plus the owner, the scope, the capability reference
    /// and the content hash.
    /// </summary>
    public static IReadOnlyList<TransitionContractDiagnostic> Admit(TransitionAdmissionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var diagnostics = new List<TransitionContractDiagnostic>(Validate(request.Document));

        if (!string.IsNullOrWhiteSpace(request.ExpectedOwner)
            && !string.Equals(request.Document.Plugin, request.ExpectedOwner, StringComparison.Ordinal))
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.OwnerMismatch,
                $"The transition owner '{request.Document.Plugin}' does not match the owner '{request.ExpectedOwner}'.",
                "plugin"));

        if (request.Scope is { } scope && !scope.IsValid)
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.ScopeInvalid,
                "The plugin scope has no host.",
                "scope"));

        if (request.ExpectedScope is { } expected && request.Scope is { } actual && !ScopeEquals(actual, expected))
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.ScopeInvalid,
                "The plugin scope does not match the scope of the host.",
                "scope"));

        ValidateCapability(request.Document.Capability, request.KnownCapabilities, diagnostics);
        ValidateContentHash(request.Document, request.DeclaredContentHash, diagnostics);
        return diagnostics;
    }

    /// <summary>Whether two scopes name the same scope.</summary>
    public static bool ScopeEquals(PortablePluginScope left, PortablePluginScope right)
        => string.Equals(left.Host, right.Host, StringComparison.Ordinal)
           && string.Equals(left.Tenant, right.Tenant, StringComparison.Ordinal)
           && string.Equals(left.Client, right.Client, StringComparison.Ordinal)
           && string.Equals(left.Session, right.Session, StringComparison.Ordinal)
           && string.Equals(left.FutureCursor, right.FutureCursor, StringComparison.Ordinal);

    private static void ValidateUseCase(TransitionUseCase? useCase, List<TransitionContractDiagnostic> diagnostics)
    {
        if (useCase is null)
        {
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.UseCaseMissing,
                "The transition use case is missing.",
                "aspects.useCase"));
            return;
        }

        if (string.IsNullOrWhiteSpace(useCase.Intent))
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.UseCaseInvalid,
                "The transition use case intent is missing.",
                "aspects.useCase.intent"));

        if (useCase.Cases is null || useCase.Cases.Count == 0)
        {
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.UseCaseInvalid,
                "The transition use case has no case.",
                "aspects.useCase.cases"));
            return;
        }

        foreach (var resultCase in useCase.Cases)
        {
            if (string.IsNullOrWhiteSpace(resultCase.Key))
                diagnostics.Add(new TransitionContractDiagnostic(
                    TransitionContractDiagnosticCodes.UseCaseInvalid,
                    "A transition result case has no key.",
                    "aspects.useCase.cases"));
            if (string.IsNullOrWhiteSpace(resultCase.Outcome))
                diagnostics.Add(new TransitionContractDiagnostic(
                    TransitionContractDiagnosticCodes.UseCaseInvalid,
                    "A transition result case has no outcome.",
                    "aspects.useCase.cases"));
            if (resultCase.Input is not { ValueKind: JsonValueKind.Object })
                diagnostics.Add(new TransitionContractDiagnostic(
                    TransitionContractDiagnosticCodes.UseCaseInvalid,
                    "A transition result case input is not an object.",
                    "aspects.useCase.cases"));
            if (resultCase.Given is null)
                diagnostics.Add(new TransitionContractDiagnostic(
                    TransitionContractDiagnosticCodes.UseCaseInvalid,
                    "A transition result case has no given state.",
                    "aspects.useCase.cases"));
        }

        var distinct = useCase.Cases.Select(resultCase => resultCase.Key).Distinct(StringComparer.Ordinal).Count();
        if (distinct != useCase.Cases.Count)
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.UseCaseDuplicate,
                "The transition use case has duplicate case keys.",
                "aspects.useCase.cases"));
    }

    private static void ValidateAuthentication(string? authentication, List<TransitionContractDiagnostic> diagnostics)
    {
        if (authentication is not ("required" or "optional"))
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.AuthenticationInvalid,
                $"The transition authentication '{authentication}' is invalid.",
                "aspects.authentication"));
    }

    private static void ValidateIntent(string? intent, List<TransitionContractDiagnostic> diagnostics)
    {
        if (intent is not null && string.IsNullOrWhiteSpace(intent))
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.IntentInvalid,
                "The transition intent is empty.",
                "aspects.intent"));
    }

    private static void ValidateInputs(
        IReadOnlyDictionary<string, TransitionPrecondition>? inputs,
        List<TransitionContractDiagnostic> diagnostics)
    {
        if (inputs is null) return;
        foreach (var precondition in inputs)
        {
            var rule = precondition.Value;
            if (rule is null)
            {
                diagnostics.Add(new TransitionContractDiagnostic(
                    TransitionContractDiagnosticCodes.InputsInvalid,
                    "A transition precondition has no rule.",
                    "aspects.inputs"));
                continue;
            }

            if (string.IsNullOrWhiteSpace(precondition.Key)
                || rule.Type is not ("string" or "boolean" or "number" or "integer" or "object" or "array"))
            {
                diagnostics.Add(new TransitionContractDiagnostic(
                    TransitionContractDiagnosticCodes.InputsInvalid,
                    $"The transition precondition '{precondition.Key}' has an unsupported type '{rule.Type}'.",
                    "aspects.inputs"));
                continue;
            }

            if (rule.Type != "string" && (rule.MinLength is not null || rule.MaxLength is not null || rule.Format is not null))
                diagnostics.Add(new TransitionContractDiagnostic(
                    TransitionContractDiagnosticCodes.InputsInvalid,
                    $"The transition precondition '{precondition.Key}' carries a length or a format rule that belongs to the string type alone.",
                    "aspects.inputs"));

            if (rule.MinLength is < 0 || rule.MaxLength is < 0 || rule.MinLength > rule.MaxLength)
                diagnostics.Add(new TransitionContractDiagnostic(
                    TransitionContractDiagnosticCodes.InputsInvalid,
                    $"The transition precondition '{precondition.Key}' has an invalid length range.",
                    "aspects.inputs"));

            if (rule.Format is not (null or "uuid"))
                diagnostics.Add(new TransitionContractDiagnostic(
                    TransitionContractDiagnosticCodes.InputsInvalid,
                    $"The transition precondition '{precondition.Key}' has an unsupported format '{rule.Format}'.",
                    "aspects.inputs"));
        }
    }

    private static void ValidateLayout(ClientLayout? layout, List<TransitionContractDiagnostic> diagnostics)
    {
        if (layout is null) return;
        if (layout.Cols < 1 || layout.Rows < 1)
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.LayoutInvalid,
                "The transition layout needs at least one column and one row.",
                "aspects.layout"));
        if (layout.Columns is null || layout.Columns.Count != layout.Cols)
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.LayoutInvalid,
                "The transition layout does not declare one column per column.",
                "aspects.layout.columns"));
        if (layout.Columns?.Any(string.IsNullOrWhiteSpace) == true)
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.LayoutInvalid,
                "The transition layout declares an empty column.",
                "aspects.layout.columns"));
        if (layout.RowSizes is { } sizes && (sizes.Count != layout.Rows || sizes.Any(string.IsNullOrWhiteSpace)))
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.LayoutInvalid,
                "The transition layout does not declare one row size per row.",
                "aspects.layout.rowSizes"));
    }

    private static void ValidateContainer(ClientContainer? container, List<TransitionContractDiagnostic> diagnostics)
    {
        if (container is null) return;
        if (string.IsNullOrWhiteSpace(container.Renderer))
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.ContainerInvalid,
                "The transition container has no renderer.",
                "aspects.container.renderer"));
        if (container.ColStart < 0 || container.RowStart < 0)
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.ContainerInvalid,
                "The transition container starts before the first column or row.",
                "aspects.container"));
        if (container.ColEnd <= container.ColStart)
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.ContainerInvalid,
                "The transition container does not span a column.",
                "aspects.container"));
        if (container.RowEnd <= container.RowStart)
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.ContainerInvalid,
                "The transition container does not span a row.",
                "aspects.container"));
    }

    private static void ValidateTriggers(
        IReadOnlyDictionary<string, TransitionTrigger>? triggers,
        List<TransitionContractDiagnostic> diagnostics)
    {
        if (triggers is null) return;
        if (triggers.Count != 2)
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.TriggersInvalid,
                "A transition carries exactly the start and the end trigger.",
                "aspects.triggers"));
        if (!triggers.TryGetValue("::start", out var start)
            || start.Effect is not ("graph.walk" or "component.list" or "server"))
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.TriggersInvalid,
                "The start trigger of a transition carries the walk, the list or the server effect.",
                "aspects.triggers.::start"));
        if (!triggers.TryGetValue("::end", out var end) || end.Effect != CapabilityReference.EndEffect)
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.TriggersInvalid,
                $"The end trigger of a transition carries the {CapabilityReference.EndEffect} effect.",
                "aspects.triggers.::end"));
    }

    private static void ValidateStateMachine(
        TransitionStateMachine? machine,
        List<TransitionContractDiagnostic> diagnostics)
    {
        if (machine is null) return;
        if (machine.Transitions is null || machine.Transitions.Count != 1)
        {
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.StateMachineInvalid,
                "A graph transition requires one machine action.",
                "aspects.stateMachine.transitions"));
            return;
        }

        var states = machine.States ?? [];
        if (states.Count == 0
            || !states.Contains(machine.Initial, StringComparer.Ordinal)
            || states.Distinct(StringComparer.Ordinal).Count() != states.Count)
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.StateMachineInvalid,
                "A graph transition requires declared states and the initial state among them.",
                "aspects.stateMachine.states"));

        var action = machine.Transitions[0];
        if (action is null)
        {
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.StateMachineInvalid,
                "A graph transition requires one machine action.",
                "aspects.stateMachine.transitions"));
            return;
        }

        if (string.IsNullOrWhiteSpace(action.Key)
            || !states.Contains(action.From, StringComparer.Ordinal)
            || !states.Contains(action.To, StringComparer.Ordinal))
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.StateMachineInvalid,
                "A graph transition requires a machine action with a key and declared source and target states.",
                "aspects.stateMachine.transitions"));

        if (action.InputSchema is null || action.InputSchema.Type != "object")
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.StateMachineInvalid,
                "The input schema of a machine action is an object.",
                "aspects.stateMachine.transitions.inputSchema"));

        if (action.InputSchema?.Required is { } required
            && action.InputSchema.Properties is { } properties
            && required.Any(key => !properties.ContainsKey(key)))
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.StateMachineInvalid,
                "The input schema of a machine action requires a property that it does not declare.",
                "aspects.stateMachine.transitions.inputSchema.required"));

        var steps = action.Steps ?? [];
        if (steps.Count(step => StepOperation(step) == "execute") != 1)
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.StateMachineInvalid,
                "A machine action executes exactly once.",
                "aspects.stateMachine.transitions.steps"));

        foreach (var step in steps)
        {
            var operation = StepOperation(step);
            if (operation is not ("execute" or "set" or "emit"))
            {
                diagnostics.Add(new TransitionContractDiagnostic(
                    TransitionContractDiagnosticCodes.StateMachineInvalid,
                    "A machine step is an execute, a set or an emit.",
                    "aspects.stateMachine.transitions.steps"));
                continue;
            }

            if (operation == "set" && !StepMember(step, "path"))
                diagnostics.Add(new TransitionContractDiagnostic(
                    TransitionContractDiagnosticCodes.StateMachineInvalid,
                    "A machine step that sets a value names the path it sets.",
                    "aspects.stateMachine.transitions.steps"));
            if (operation == "emit" && !StepMember(step, "key"))
                diagnostics.Add(new TransitionContractDiagnostic(
                    TransitionContractDiagnosticCodes.StateMachineInvalid,
                    "A machine step that emits an event names the key it emits.",
                    "aspects.stateMachine.transitions.steps"));
        }
    }

    private static string? StepOperation(JsonElement step)
    {
        if (step.ValueKind != JsonValueKind.Object) return null;
        if (!step.TryGetProperty("op", out var operation) || operation.ValueKind != JsonValueKind.String) return null;
        return operation.GetString();
    }

    private static bool StepMember(JsonElement step, string member)
        => step.ValueKind == JsonValueKind.Object
           && step.TryGetProperty(member, out var value)
           && value.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined);

    private static void ValidateCapability(
        string? capability,
        IReadOnlyCollection<string>? knownCapabilities,
        List<TransitionContractDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(capability)) return;
        if (knownCapabilities is null || knownCapabilities.Count == 0) return;
        if (knownCapabilities.Contains(capability, StringComparer.Ordinal)) return;

        var reference = CapabilityReference.Parse(capability);
        diagnostics.Add(new TransitionContractDiagnostic(
            TransitionContractDiagnosticCodes.CapabilityUnknown,
            $"Capability '{capability}' is not registered.",
            "capability"));

        if (reference.IsKernel && !reference.IsFixed)
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.CapabilityMisclassified,
                $"The kernel capability '{capability}' is not one of the fixed capabilities of the platform.",
                "capability"));
    }

    private static void ValidateContentHash(
        TransitionDefinitionDocument document,
        PortablePluginContentHash? declared,
        List<TransitionContractDiagnostic> diagnostics)
    {
        if (declared is null) return;
        if (!PortablePluginContentHash.IsSupported(declared.Algorithm))
        {
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.HashAlgorithmUnknown,
                $"The content hash algorithm '{declared.Algorithm}' is not supported.",
                "contentHash.algorithm"));
            return;
        }

        var canonical = TransitionContractDocument.Canonicalize(document);
        var actual = PortablePluginContentHash.Compute(declared.Algorithm, canonical);
        if (!string.Equals(actual.Digest, declared.Digest, StringComparison.OrdinalIgnoreCase))
            diagnostics.Add(new TransitionContractDiagnostic(
                TransitionContractDiagnosticCodes.HashMismatch,
                $"The content hash does not match the definition. The {actual.Algorithm} digest of the definition is '{actual.Digest}'.",
                "contentHash.digest"));
    }
}
