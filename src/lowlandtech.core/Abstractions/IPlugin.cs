namespace LowlandTech.Core.Abstractions;

public interface IPlugin
{
    public Guid Id { get; }
    public string Name { get; set; }
    bool IsActive { get; set; }
    public string? Description { get; }
    public string? Company { get; }
    public string? Copyright { get; }
    public string? Url { get; }
    public string? Version { get; }
    public string? Authors { get; }
    public List<Assembly>? Assemblies { get; }

    void Install(ServiceRegistry services);
    void Configure(WebApplication app);
}
