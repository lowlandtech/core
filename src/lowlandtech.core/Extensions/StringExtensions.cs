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

    /// <summary>
    /// Attempts to pluralize the specified text according to the rules of the English language.
    /// </summary>
    /// <remarks>
    /// This function attempts to pluralize as many words as practical by following these rules:
    /// <list type="bullet">
    ///		<item><description>Words that don't follow any rules (e.g. "mouse" becomes "mice") are returned from a dictionary.</description></item>
    ///		<item><description>Words that end with "y" (but not with a vowel preceding the y) are pluralized by replacing the "y" with "ies".</description></item>
    ///		<item><description>Words that end with "us", "ss", "x", "ch" or "sh" are pluralized by adding "es" to the end of the text.</description></item>
    ///		<item><description>Words that end with "f" or "fe" are pluralized by replacing the "f(e)" with "ves".</description></item>
    ///	</list>
    /// </remarks>
    /// <param name="text">The text to pluralize.</param>
    /// <param name="number">If number is 1, the text is not pluralized; otherwise, the text is pluralized.</param>
    /// <returns>A string that consists of the text in its pluralized form.</returns>
    public static string ToPlural(this string text, int number = 2)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        if (number == 1)
        {
            return text;
        }
        else
        {
            // Create a dictionary of exceptions that have to be checked first
            Dictionary<string, string> exceptions = new() {
            { "man", "men" },
            { "woman", "women" },
            { "child", "children" },
            { "tooth", "teeth" },
            { "foot", "feet" },
            { "mouse", "mice" },
            { "belief", "beliefs" }
        };

            if (exceptions.TryGetValue(text.ToLowerInvariant(), out string? value))
            {
                return value;
            }
            else if (text.EndsWith("y", StringComparison.OrdinalIgnoreCase) &&
                     !text.EndsWith("ay", StringComparison.OrdinalIgnoreCase) &&
                     !text.EndsWith("ey", StringComparison.OrdinalIgnoreCase) &&
                     !text.EndsWith("iy", StringComparison.OrdinalIgnoreCase) &&
                     !text.EndsWith("oy", StringComparison.OrdinalIgnoreCase) &&
                     !text.EndsWith("uy", StringComparison.OrdinalIgnoreCase))
            {
                return string.Concat(text.AsSpan(0, text.Length - 1), "ies");
            }
            else if (text.EndsWith("us", StringComparison.InvariantCultureIgnoreCase))
            {
                return text + "es";
            }
            else if (text.EndsWith("ss", StringComparison.InvariantCultureIgnoreCase))
            {
                return text + "es";
            }
            else if (text.EndsWith("s", StringComparison.InvariantCultureIgnoreCase))
            {
                return text;
            }
            else if (text.EndsWith("x", StringComparison.InvariantCultureIgnoreCase) ||
                     text.EndsWith("ch", StringComparison.InvariantCultureIgnoreCase) ||
                     text.EndsWith("sh", StringComparison.InvariantCultureIgnoreCase))
            {
                return text + "es";
            }
            else if (text.EndsWith("f", StringComparison.InvariantCultureIgnoreCase) && text.Length > 1)
            {
                return string.Concat(text.AsSpan(0, text.Length - 1), "ves");
            }
            else if (text.EndsWith("fe", StringComparison.InvariantCultureIgnoreCase) && text.Length > 2)
            {
                return string.Concat(text.AsSpan(0, text.Length - 2), "ves");
            }
            else
            {
                return text + "s";
            }
        }
    }

    /// <summary>
    /// Transforms the specified string to camel case.
    /// </summary>
    /// <param name="str">The string to be converted</param>
    /// <returns></returns>
    public static string ToCamelCase(this string str)
    {
        if (!string.IsNullOrEmpty(str) && str.Length > 1)
        {
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
        return str.ToLowerInvariant();
    }
}
