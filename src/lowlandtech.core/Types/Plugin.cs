namespace LowlandTech.Core.Types;

public abstract class Plugin : IPlugin
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; }
    public string? Company { get; }
    public string? Copyright { get; }
    public string? Url { get; }
    public string? Version { get; }
    public string? Authors { get; }
    [JsonIgnore]
    public List<Assembly> Assemblies { get; } = new();
    public abstract void Install(ServiceRegistry services);
    public abstract void Configure(WebApplication app);
}
