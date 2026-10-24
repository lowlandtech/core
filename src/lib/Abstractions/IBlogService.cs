
namespace LowlandTech.Domain.Abstractions;

public interface IBlogService
{
    Task<List<Blog>> ListAsync(CancellationToken ct = default);
    Task<Blog?> GetByUserAsync(string userId, CancellationToken ct = default);
    Task<Blog?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Blog> CreateAsync(Blog blog, CancellationToken ct = default);
    Task<List<Post>> ListPublishedPostsAsync(CancellationToken ct = default);
    Task<Post?> GetPostBySlugAsync(string username, string slug, CancellationToken ct = default);
    Task<List<Post>> ListPostsByUsernameAsync(string username, bool includeUnpublished = false, CancellationToken ct = default);
    Task<List<Bookmark>> ListBookmarksAsync(string userId, CancellationToken ct = default);
    Task<bool> ToggleBookmarkAsync(string userId, int postId, CancellationToken ct = default);
}
