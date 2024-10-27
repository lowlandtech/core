namespace LowlandTech.Core.Extensions;

public static class GuardExtensions
{
    public static string MissingFile(this IGuardClause guardClause, string file, string parameterName, string message = $"Invalid file, it must exist")
    {
        Guard.Against.NullOrEmpty(file, nameof(file));

        if (!File.Exists(file))
        {
            throw new ArgumentException(message, parameterName);
        }

        return file;
    }

    public static string MissingFolder(this IGuardClause guardClause, string folder, string parameterName, string message = $"Invalid folder, it must exist")
    {
        Guard.Against.NullOrEmpty(folder, nameof(folder));

        if (!Directory.Exists(folder))
        {
            throw new ArgumentException(message, parameterName);
        }

        return folder;
    }

    public static string MissingPluginId(this IGuardClause guardClause, IPlugin plugin, string parameterName, string message = $"Invalid plugin id, it must be provided")
    {
        Guard.Against.Null(plugin, nameof(plugin));

        if (Attribute.GetCustomAttribute(plugin.GetType(), typeof(PluginId)) is not PluginId pluginId)
        {
            throw new ArgumentException(message, parameterName);
        }

        return pluginId.Id;
    }
}
