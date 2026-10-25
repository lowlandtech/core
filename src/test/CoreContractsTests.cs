namespace LowlandTech.Core.Tests;

public sealed class CoreContractsTests
{
    [Fact]
    public void RegistryPreservesDescriptorIdentityAndOrdering()
    {
        var bound = new BoundNodeDescriptor();
        var registry = new NodeTypeRegistry([bound, new HeaderNodeType()]);
        registry.All.Select(entry => entry.Key).ShouldBe(["header", "extraction.bound"]);
        registry.FindByEntityType(typeof(Node))!.Id.ShouldBe(bound.Id);
    }

    [Fact]
    public void EntityProjectionPreservesStaticDescriptorIdentity()
    {
        var registry = new NodeTypeRegistry([new HeaderNodeType()]);
        var entity = NodeTypeRegistry.ToEntity(registry.All.Single());
        entity.Id.ShouldBe(HeaderNodeType.NodeTypeId);
        entity.IsStatic.ShouldBeTrue();
    }

    [Fact]
    public void DuplicateBoundDescriptorsFailBeforeLookup()
        => Should.Throw<InvalidOperationException>(() =>
            new NodeTypeRegistry([new BoundNodeDescriptor(), new BoundNodeDescriptor()]))
            .Message.ShouldContain("Multiple INodeTypeDescriptor");

    [Fact]
    public void ExistingPluginDefaultRemainsRouted()
        => new PluginDescriptor { Id = "extraction.test", Name = "Test", Version = "1.0.0" }
            .InstallModes.ShouldBe([PluginInstallMode.Routed]);
}
