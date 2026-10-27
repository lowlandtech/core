using System.Text.Json;
using System.Text.Json.Serialization;

namespace LowlandTech.Core.Plugins;

/// <summary>
/// Approval state of a <see cref="GraphMergeRequest"/>.
/// SPEC2050 US05; CORE0005-US01-SC01.
/// </summary>
/// <remarks>
/// The numeric value of a member is its stable wire value. The three published
/// members keep their original numbers and their original names, so a document
/// written before the reviewing, the merged and the abandoned state still
/// deserializes to its original meaning.
/// </remarks>
[JsonConverter(typeof(MergeRequestStatusWireConverter))]
public enum MergeRequestStatus
{
    /// <summary>The request waits for its first review.</summary>
    Open = 0,

    /// <summary>The request was approved and its transitions were replayed.</summary>
    Approved = 1,

    /// <summary>The request was rejected and its branch was discarded.</summary>
    Rejected = 2,

    /// <summary>The request is under review.</summary>
    Reviewing = 3,

    /// <summary>The approved request was merged into its target branch.</summary>
    Merged = 4,

    /// <summary>The request was abandoned and its branch was discarded.</summary>
    Abandoned = 5,
}

/// <summary>
/// The lifecycle rules of a <see cref="MergeRequestStatus"/>.
/// </summary>
/// <remarks>
/// The rules live with the status so that the client that renders a request and
/// the processor that moves it read the same rule and cannot drift apart.
/// </remarks>
public static class MergeRequestStatusLifecycle
{
    /// <summary>Whether a request in this state may still be approved.</summary>
    public static bool CanApprove(this MergeRequestStatus status)
        => status is MergeRequestStatus.Open or MergeRequestStatus.Reviewing;

    /// <summary>Whether a request in this state may still be rejected.</summary>
    public static bool CanReject(this MergeRequestStatus status)
        => status is MergeRequestStatus.Open or MergeRequestStatus.Reviewing;

    /// <summary>Whether a request in this state may enter the review.</summary>
    public static bool CanStartReview(this MergeRequestStatus status)
        => status is MergeRequestStatus.Open;

    /// <summary>Whether a request in this state may still be abandoned.</summary>
    public static bool CanAbandon(this MergeRequestStatus status)
        => status is MergeRequestStatus.Open or MergeRequestStatus.Reviewing;

    /// <summary>Whether a request in this state may be merged.</summary>
    public static bool CanMerge(this MergeRequestStatus status)
        => status is MergeRequestStatus.Approved;

    /// <summary>Whether the request reached a state it never leaves.</summary>
    public static bool IsTerminal(this MergeRequestStatus status)
        => status is MergeRequestStatus.Rejected or MergeRequestStatus.Merged
            or MergeRequestStatus.Abandoned;
}

/// <summary>
/// Reads and writes the wire values of a <see cref="MergeRequestStatus"/>.
/// </summary>
/// <remarks>
/// The converter writes the stable numeric wire value and reads that numeric
/// value as well as the member name, so a document of an older producer and a
/// document of a client that prefers names both keep their meaning. A value
/// that names no state is refused instead of being read as an unknown state.
/// </remarks>
public sealed class MergeRequestStatusWireConverter : JsonConverter<MergeRequestStatus>
{
    /// <inheritdoc />
    public override MergeRequestStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out var value))
            {
                var status = (MergeRequestStatus)value;
                if (Enum.IsDefined(status)) return status;
                throw new JsonException($"'{value}' is not a merge-request status.");
            }
            throw new JsonException("A merge-request status is a number or a name.");
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var text = reader.GetString();
            if (!string.IsNullOrEmpty(text)
                && Enum.TryParse<MergeRequestStatus>(text, ignoreCase: true, out var parsed)
                && Enum.IsDefined(parsed))
                return parsed;
            throw new JsonException($"'{text}' is not a merge-request status.");
        }

        throw new JsonException($"A merge-request status is a number or a name, not {reader.TokenType}.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, MergeRequestStatus value, JsonSerializerOptions options)
        => writer.WriteNumberValue((int)value);
}
