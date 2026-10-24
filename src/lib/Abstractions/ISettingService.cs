namespace LowlandTech.Domain.Abstractions;

public interface ISettingService
{
    Task<ResolvedSetting> GetCurrentAsync(string key, string defaultValue, CancellationToken ct = default);
    Task<ResolvedSetting> SetCurrentAsync(string key, string value, string defaultValue, CancellationToken ct = default);
}

public sealed record ResolvedSetting(string Key, string DefaultValue, string Value, bool HasOverride);