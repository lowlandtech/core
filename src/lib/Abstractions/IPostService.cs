
namespace LowlandTech.Domain.Abstractions;

public interface IPostService
{
	Task<List<Post>> ListAsync(int? blogId = null, bool? published = null, CancellationToken ct = default);
	Task<Post?> GetAsync(int id, CancellationToken ct = default);
	Task<Post> CreateAsync(Post post, CancellationToken ct = default);
	Task<Post?> UpdateAsync(int id, Action<Post> applyChanges, CancellationToken ct = default);
	Task<bool> DeleteAsync(int id, CancellationToken ct = default);
	Task<Post?> PublishAsync(int id, CancellationToken ct = default);
	Task<Post?> UnpublishAsync(int id, CancellationToken ct = default);
}