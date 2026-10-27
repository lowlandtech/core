namespace LowlandTech.Core.Contracts.Transitions;

/// <summary>
/// One named refusal of an inadmissible transition document. The code is the
/// stable identity of the refusal and the message names it in the words of the
/// host, so a refusal keeps its name across the cutover.
/// </summary>
public sealed record TransitionContractDiagnostic(string Code, string Message, string Member)
{
    /// <summary>The refusal as one line of a report.</summary>
    public override string ToString() => $"{Code}: {Message} ({Member})";
}

/// <summary>The stable codes of the refusals of the contract.</summary>
public static class TransitionContractDiagnosticCodes
{
    /// <summary>The document is empty or absent.</summary>
    public const string DocumentEmpty = "gs.contract.document.empty";

    /// <summary>The schema of the document is not the supported schema.</summary>
    public const string SchemaUnsupported = "gs.contract.schema.unsupported";

    /// <summary>The kind of the document is not the supported kind.</summary>
    public const string KindUnsupported = "gs.contract.kind.unsupported";

    /// <summary>The owner of the document is missing.</summary>
    public const string OwnerMissing = "gs.contract.owner.missing";

    /// <summary>The owner of the document does not own the transition.</summary>
    public const string OwnerMismatch = "gs.contract.owner.mismatch";

    /// <summary>The identity of the document is missing.</summary>
    public const string IdentityMissing = "gs.contract.identity.missing";

    /// <summary>The identity of the document belongs to another definition.</summary>
    public const string IdentityConflict = "gs.contract.identity.conflict";

    /// <summary>A required field of the document is missing.</summary>
    public const string FieldMissing = "gs.contract.field.missing";

    /// <summary>The aspects of the document are missing.</summary>
    public const string AspectsMissing = "gs.contract.aspects.missing";

    /// <summary>The use case of the document is missing.</summary>
    public const string UseCaseMissing = "gs.contract.useCase.missing";

    /// <summary>The use case of the document is invalid.</summary>
    public const string UseCaseInvalid = "gs.contract.useCase.invalid";

    /// <summary>The use case of the document has duplicate case keys.</summary>
    public const string UseCaseDuplicate = "gs.contract.useCase.duplicate";

    /// <summary>The authentication of the document is invalid.</summary>
    public const string AuthenticationInvalid = "gs.contract.authentication.invalid";

    /// <summary>The intent of the document is invalid.</summary>
    public const string IntentInvalid = "gs.contract.intent.invalid";

    /// <summary>The input schema of the document is invalid.</summary>
    public const string InputsInvalid = "gs.contract.inputs.invalid";

    /// <summary>The layout of the document is invalid.</summary>
    public const string LayoutInvalid = "gs.contract.layout.invalid";

    /// <summary>The container of the document is invalid.</summary>
    public const string ContainerInvalid = "gs.contract.container.invalid";

    /// <summary>The triggers of the document are invalid.</summary>
    public const string TriggersInvalid = "gs.contract.triggers.invalid";

    /// <summary>The state machine of the document is invalid.</summary>
    public const string StateMachineInvalid = "gs.contract.stateMachine.invalid";

    /// <summary>The capability reference of the document is unknown.</summary>
    public const string CapabilityUnknown = "gs.contract.capability.unknown";

    /// <summary>The capability reference of the document is misclassified.</summary>
    public const string CapabilityMisclassified = "gs.contract.capability.misclassified";

    /// <summary>The scope of the document is invalid.</summary>
    public const string ScopeInvalid = "gs.contract.scope.invalid";

    /// <summary>The content hash of the document does not match its content.</summary>
    public const string HashMismatch = "gs.contract.hash.mismatch";

    /// <summary>The content hash of the document names an unknown algorithm.</summary>
    public const string HashAlgorithmUnknown = "gs.contract.hash.algorithm.unknown";
}
