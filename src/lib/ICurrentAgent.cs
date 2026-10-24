namespace LowlandTech.Core;

public interface ICurrentAgent
{
    Guid Id { get; }
    string Name { get; }
    string Initials { get; }
}
