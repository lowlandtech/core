namespace LowlandTech.Core;

public interface IFoundryClient
{
    Task<IReadOnlyList<ContainerDto>> GetAllContainersAsync(CancellationToken ct = default);
    Task<ContainerDto?> GetContainerByNameAsync(string name, CancellationToken ct = default);
}

public record ContainerDto(
    Guid Id, string Name, string Type,
    IReadOnlyList<ContainerItemDto> Items);

public record ContainerItemDto(
    Guid Id, string Name, string? Element, string? ElementType,
    bool IsGrid, int? GridColumnStart, int? GridColumnEnd, int? GridRowStart, int? GridRowEnd,
    IReadOnlyList<ContainerItemDto> Children,
    IReadOnlyList<ContainerAttributeDto> Attributes);

public record ContainerAttributeDto(
    string Name, string Value, string DataType, string Source);
