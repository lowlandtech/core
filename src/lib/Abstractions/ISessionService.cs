
namespace LowlandTech.Domain.Abstractions;

public interface ISessionService
{
    Task<List<Session>> ListAsync(string type, Guid? typeId = null);
    Task<Session?> GetAsync(Guid id);
    Task<Session> GetOrCreateAsync(string type, Guid typeId, string name);
    Task<Session> CreateAsync(Session session);
    Task DeleteAsync(Guid id);
    Task<List<SessionItem>> GetMessagesAsync(Guid sessionId, bool includeBlocks = false);
    Task<List<SessionItem>> GetMessagesWithBlocksAsync(Guid sessionId);
    Task<SessionItem> AddMessageAsync(Guid sessionId, string userId, string value, string role = "user", Guid? parentId = null);
    Task<SessionBlock> AddBlockAsync(Guid messageId, string blockType, string data, int sortOrder = 0);
    Task<List<SessionBlock>> GetBlocksAsync(Guid messageId);
    Task DeleteMessageAsync(Guid messageId);
}
