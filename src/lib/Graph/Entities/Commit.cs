namespace LowlandTech.Core.Graph.Entities;

/// <summary>
/// An immutable record of a successful edge traversal.
/// Forms a hash chain per edge, providing causal integrity — like git commits but for graphs.
///
/// On successful traversal:
/// 1. Serialize source + target node state as JSON (excluding timestamps)
/// 2. Derive an edge key from the user's HD wallet (seeded per user node)
/// 3. Hash the state payload with the edge key
/// 4. Point to the previous commit on this edge (or null for genesis)
///
/// The chain: Commit(n).PreviousHash == Commit(n-1).Hash
/// Verifying integrity: re-derive hash from payload + edge key, compare with stored hash.
/// </summary>
public class Commit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EdgeId { get; set; }
    public Guid UserNodeId { get; set; }                    // The user node whose wallet key signed this

    public string SourceState { get; set; } = string.Empty; // JSON snapshot of source node (no timestamps)
    public string TargetState { get; set; } = string.Empty; // JSON snapshot of target node (no timestamps)

    public string Hash { get; set; } = string.Empty;        // HMAC(edgeKey, sourceState + targetState + previousHash)
    public string? PreviousHash { get; set; }               // Hash of the previous commit on this edge (null = genesis)

    public int Sequence { get; set; }                       // Monotonic counter per edge (0, 1, 2, ...)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Edge Edge { get; set; } = null!;
    public Node UserNode { get; set; } = null!;
}
