namespace LowlandTech.Core;

public interface ITheme
{
    Guid Id { get; }
    string Name { get; }
    string? ResolveToken(string tokenName);
    IReadOnlyDictionary<string, string>? GetSlotOverrides(string dataSlotValue);
    IReadOnlyDictionary<string, IReadOnlyList<ThemeTokenValue>> GetTokensByCategory();
}

public record ThemeTokenValue(string Name, string Value);
