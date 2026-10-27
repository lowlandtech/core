namespace LowlandTech.Core.Contracts.Transitions;

/// <summary>
/// One typed precondition of a transition. The length and the format rules
/// belong to the string type alone.
/// </summary>
public sealed record TransitionPrecondition(
    string Type,
    bool Required = false,
    int? MinLength = null,
    int? MaxLength = null,
    string? Format = null,
    string? Description = null,
    bool Secret = false,
    FieldPresentation? Presentation = null);

/// <summary>The presentation of a precondition field.</summary>
public sealed record FieldPresentation(
    string? Label = null,
    string? Help = null,
    int? Order = null,
    string? Group = null,
    string? ReferenceType = null);
