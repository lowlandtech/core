namespace LowlandTech.Core.Sessions;

/// <summary>DI-supplied fallback component type used when no registry handles a (nodeTypeKey, kind) pair.</summary>
public interface INodeKindDefault
{
    Type ComponentType { get; }
}
