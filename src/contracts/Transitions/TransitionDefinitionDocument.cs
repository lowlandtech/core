namespace LowlandTech.Core.Contracts.Transitions;

/// <summary>
/// The versioned, data-only transition document of a host graph.
/// </summary>
/// <remarks>
/// The member order of this record is the wire order of the document. The host
/// writes the document in this order and a round trip through the contract has
/// to reproduce it exactly, so the order is part of the contract and not an
/// implementation detail of a serializer.
/// </remarks>
public sealed record TransitionDefinitionDocument(
    string Schema,
    string Kind,
    string Plugin,
    string Key,
    string Source,
    string EdgeType,
    string Target,
    string Capability,
    TransitionAspectSet Aspects);
