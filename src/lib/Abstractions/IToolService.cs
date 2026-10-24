
namespace LowlandTech.Domain.Abstractions;

public interface IToolService
{
    Task<List<Tool>> ListAsync(CancellationToken ct = default);
    Task<Tool?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Tool> CreateAsync(Tool tool, CancellationToken ct = default);
    Task<Tool?> UpdateAsync(Tool tool, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ToolCommand> CreateCommandAsync(ToolCommand command, CancellationToken ct = default);
    Task<ToolCommand?> GetCommandAsync(Guid id, CancellationToken ct = default);
    Task<List<ToolCommand>> ListCommandsAsync(Guid toolId, CancellationToken ct = default);
    Task<ToolCommand?> UpdateCommandAsync(ToolCommand command, CancellationToken ct = default);
    Task<bool> DeleteCommandAsync(Guid id, CancellationToken ct = default);
}
