
namespace LowlandTech.Core.Graph.Types;

public class WalletService : IWalletService
{
    public string GenerateSeedPhrase()
    {
        var mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
        return mnemonic.ToString();
    }

    public byte[] DeriveEdgeKey(string seedPhrase, int edgeIndex)
    {
        var mnemonic = new Mnemonic(seedPhrase, Wordlist.English);
        var masterKey = mnemonic.DeriveExtKey();

        // BIP32 derivation: m/44'/0'/{edgeIndex}'
        var derived = masterKey
            .Derive(new KeyPath($"m/44'/0'/{edgeIndex}'"));

        return derived.PrivateKey.ToBytes();
    }

    public string ComputeHash(byte[] edgeKey, string payload)
    {
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var hash = HMACSHA256.HashData(edgeKey, payloadBytes);
        return Convert.ToHexStringLower(hash);
    }

    public bool VerifyHash(byte[] edgeKey, string payload, string expectedHash)
    {
        var computed = ComputeHash(edgeKey, payload);
        return string.Equals(computed, expectedHash, StringComparison.OrdinalIgnoreCase);
    }
}
