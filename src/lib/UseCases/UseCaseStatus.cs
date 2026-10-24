namespace LowlandTech.Core.UseCases;

public sealed record UseCaseStatus(
    Guid Id,
    string Name,
    string Type,
    bool IsSeeded,
    DateTimeOffset? LastSeededAt,
    string? LastError);
