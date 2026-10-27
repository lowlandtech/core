using System.Text.Json;

namespace LowlandTech.Core.Contracts.Transitions;

/// <summary>
/// Reads and writes the versioned transition document. The reader and the
/// writer are the canonical form of the document: the writer reproduces the
/// member order of the host, so a document that round trips through the
/// contract is byte identical to the document the host wrote.
/// </summary>
public static class TransitionContractDocument
{
    /// <summary>The schema the contract supports.</summary>
    public const string SupportedSchema = "gs.transition/v1";

    /// <summary>The kind the contract supports.</summary>
    public const string SupportedKind = "transition";

    /// <summary>Reads a transition document from its serialized form.</summary>
    /// <exception cref="InvalidDataException">The document is empty.</exception>
    public static TransitionDefinitionDocument Read(string json)
        => JsonSerializer.Deserialize(json, TransitionContractJsonContext.Default.TransitionDefinitionDocument)
           ?? throw new InvalidDataException("The transition document is empty.");

    /// <summary>Writes a transition document in its canonical serialized form.</summary>
    public static string Write(TransitionDefinitionDocument document)
        => JsonSerializer.Serialize(document, TransitionContractJsonContext.Default.TransitionDefinitionDocument);

    /// <summary>
    /// The canonical serialized form of a document. The canonical form is the
    /// byte form a content hash is computed over, so two producers of the same
    /// document agree on its hash.
    /// </summary>
    public static string Canonicalize(TransitionDefinitionDocument document) => Write(document);

    /// <summary>The canonical serialized form of a serialized document.</summary>
    public static string Canonicalize(string json) => Write(Read(json));
}
