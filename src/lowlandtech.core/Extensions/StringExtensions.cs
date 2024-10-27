namespace LowlandTech.Core.Extensions;

/// <summary>
/// Extension methods for <see cref="string"/> objects.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Computes the SHA-256 hash of the provided string.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>A string representation of the SHA-256 hash.</returns>
    public static string ComputeSha256Hash(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(bytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}
