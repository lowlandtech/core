namespace LowlandTech.Core.Graph.NodeTypes;

/// <summary>Represents a registered application use case and its seed status.</summary>
public sealed class UseCaseNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = new("a1000000-0000-0000-0000-000000000207");

    public Guid Id => NodeTypeId;
    public string Key => "use-case";
    public string Label => "Use Case";
    public string? Icon => nameof(Icons.Database);
    public string? BlockComponent => null;
    public int SortOrder => 14;
    public Type? EntityType => null;
    public bool IsListable => true;
}
