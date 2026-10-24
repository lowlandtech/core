namespace LowlandTech.Core.Graph;

/// <summary>
/// HD wallet operations for graph commit signing.
/// Each user node has a BIP39 seed phrase that generates a deterministic key hierarchy.
/// Edge keys are derived at path m/purpose'/edgeIndex' for signing commits.
/// </summary>
public interface IWalletService
{
    /// <summary>
    /// Generate a new BIP39 mnemonic seed phrase (12 words).
    /// </summary>
    string GenerateSeedPhrase();

    /// <summary>
    /// Derive a deterministic key for a specific edge from the user's seed phrase.
    /// Uses BIP32 derivation path: m/44'/0'/{edgeIndex}'
    /// </summary>
    byte[] DeriveEdgeKey(string seedPhrase, int edgeIndex);

    /// <summary>
    /// Compute HMAC-SHA256 of the payload using the derived edge key.
    /// </summary>
    string ComputeHash(byte[] edgeKey, string payload);

    /// <summary>
    /// Verify that a hash matches the payload signed with the given edge key.
    /// </summary>
    bool VerifyHash(byte[] edgeKey, string payload, string expectedHash);
}
