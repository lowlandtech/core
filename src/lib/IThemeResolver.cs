namespace LowlandTech.Core;

public interface IThemeResolver
{
    Task<ITheme?> GetDefaultThemeAsync(CancellationToken ct = default);
    Task<ITheme?> GetThemeForContainerAsync(Guid containerId, CancellationToken ct = default);
    Task ApplyThemeAsync(Guid containerId, Guid themeId, CancellationToken ct = default);
}
