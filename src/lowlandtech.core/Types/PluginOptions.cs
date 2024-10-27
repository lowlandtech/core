namespace LowlandTech.Core.Types;

public class PluginOptions
{
    public const string Name = "Plugins";

    public List<PluginConfig> Plugins { get; set; } = [];
}