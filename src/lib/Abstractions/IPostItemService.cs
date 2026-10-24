
namespace LowlandTech.Domain.Abstractions;

public interface IPostItemService
{
	Task<List<PostItem>> ListAsync(int postId, CancellationToken ct = default);
	Task<PostItem?> CreateAsync(int postId, PostItem item, CancellationToken ct = default);
	Task<PostItem?> UpdateAsync(int postId, int itemId, Action<PostItem> applyChanges, CancellationToken ct = default);
	Task<bool> DeleteAsync(int postId, int itemId, CancellationToken ct = default);
}