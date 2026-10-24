
namespace LowlandTech.Core.Graph;

/// <summary>
/// Creates cryptographic commits after successful edge traversals.
///
/// Flow:
/// 1. Snapshot source + target node state as JSON (strip timestamps)
/// 2. Derive edge key from user's HD wallet (user node has seed phrase property)
/// 3. HMAC(edgeKey, sourceState + targetState + previousHash)
/// 4. Persist Commit with hash chain pointer
///
/// The commit chain per edge is: genesis → c0 → c1 → c2 → ...
/// Each commit's PreviousHash == the prior commit's Hash (null for genesis).
/// Integrity check: recompute hash from payload + key, compare with stored hash.
/// </summary>
public interface ICommitService
{
    /// <summary>
    /// Record a commit for a successful edge traversal.
    /// </summary>
    Task<Commit> CommitAsync(Edge edge, Node sourceNode, Node targetNode, Guid userNodeId, CancellationToken ct = default);

    /// <summary>
    /// Verify the integrity of the commit chain for an edge.
    /// Returns false if any hash in the chain doesn't match recomputation.
    /// </summary>
    Task<bool> VerifyChainAsync(Guid edgeId, CancellationToken ct = default);

    /// <summary>
    /// Get the commit history for an edge, ordered by sequence.
    /// </summary>
    Task<IReadOnlyList<Commit>> GetHistoryAsync(Guid edgeId, CancellationToken ct = default);
}
