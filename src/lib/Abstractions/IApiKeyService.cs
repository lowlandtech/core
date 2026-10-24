
namespace LowlandTech.Domain.Abstractions;

public interface IApiKeyService
{
    Task<List<ApiKey>> ListByUserAsync(string userId, CancellationToken ct = default);
    Task<ApiKey> CreateAsync(ApiKey apiKey, CancellationToken ct = default);
    Task<bool> RevokeAsync(int id, CancellationToken ct = default);
}
