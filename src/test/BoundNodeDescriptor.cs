namespace LowlandTech.Core.Tests;

internal sealed class BoundNodeDescriptor : INodeTypeDescriptor
{
    public Guid Id => Guid.Parse("0199ff11-1000-7000-8000-000000000001");
    public string Key => "extraction.bound";
    public string Label => "Bound node";
    public string? Icon => null;
    public string? BlockComponent => null;
    public int SortOrder => 20;
    public Type? EntityType => typeof(Node);
}
