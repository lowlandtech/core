
using System.Diagnostics.CodeAnalysis;

namespace LowlandTech.Core.Plugins;

/// <summary>
/// Discovers <see cref="IApiPlugin"/> and <see cref="IAppPlugin"/> implementations
/// via three ordered strategies:
/// <list type="number">
///   <item>All currently loaded assemblies (catches built-in, compile-time referenced plugins)</item>
///   <item>Assembly paths declared in <c>Plugins:Backend:Assemblies</c> / <c>Plugins:Frontend:Assemblies</c></item>
///   <item>Assembly-qualified type names in <c>Plugins:Backend:Types</c> / <c>Plugins:Frontend:Types</c></item>
///   <item>Directory scan of <c>Plugins:Backend:PackagesPath</c> / <c>Plugins:Frontend:PackagesPath</c> (NuGet package folder)</item>
/// </list>
/// Each strategy deduplicates by concrete type so the same plugin is never registered twice.
/// </summary>
public static class PluginDiscovery
{
    /// <summary>
    /// Discovers the API plugins of the host.
    /// <para>
    /// The discovery scans loaded assemblies and loads assemblies by path, so the
    /// discovered plugin types are not statically reachable. The entrypoint
    /// therefore requires unreferenced-code handling, and a trimmed host must
    /// call it through a dynamic-code boundary.
    /// </para>
    /// </summary>
    [RequiresUnreferencedCode(
        "Plugin discovery loads plugin assemblies and instantiates plugin types by reflection, so the plugin types are not statically reachable and cannot be preserved by the trimmer.")]
    public static IReadOnlyList<IApiPlugin> DiscoverApiPlugins(IConfiguration configuration)
        => Discover<IApiPlugin>(configuration, "Plugins:Backend");

    /// <summary>
    /// Discovers the app plugins of the host.
    /// <para>
    /// The discovery scans loaded assemblies and loads assemblies by path, so the
    /// discovered plugin types are not statically reachable. The entrypoint
    /// therefore requires unreferenced-code handling, and a trimmed host must
    /// call it through a dynamic-code boundary.
    /// </para>
    /// </summary>
    [RequiresUnreferencedCode(
        "Plugin discovery loads plugin assemblies and instantiates plugin types by reflection, so the plugin types are not statically reachable and cannot be preserved by the trimmer.")]
    public static IReadOnlyList<IAppPlugin> DiscoverAppPlugins(IConfiguration configuration)
        => Discover<IAppPlugin>(configuration, "Plugins:Frontend");

    [RequiresUnreferencedCode(
        "Plugin discovery loads plugin assemblies and instantiates plugin types by reflection, so the plugin types are not statically reachable and cannot be preserved by the trimmer.")]
    private static IReadOnlyList<T> Discover<T>(IConfiguration configuration, string section)
    {
        var plugins = new List<T>();
        var seen = new HashSet<Type>();

        // 1. Transitively force-load all assemblies reachable from the entry assembly so
        //    that plugin assemblies buried deep in the dependency tree are in the domain
        //    before we scan. A simple one-level load misses plugins in indirect deps
        //    (e.g. CLI → Infrastructure → Web → Shell → Auth.Frontend / Onboarding.Frontend).
        var loadedNames = new HashSet<string>(
            AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetName().Name ?? ""),
            StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<AssemblyName>(
            Assembly.GetEntryAssembly()?.GetReferencedAssemblies() ?? []);
        while (queue.Count > 0)
        {
            var refName = queue.Dequeue();
            if (refName.Name is null || loadedNames.Contains(refName.Name)) continue;
            loadedNames.Add(refName.Name);
            try
            {
                var asm = Assembly.Load(refName);
                foreach (var transitive in asm.GetReferencedAssemblies())
                    if (transitive.Name is not null && !loadedNames.Contains(transitive.Name))
                        queue.Enqueue(transitive);
            }
            catch { /* skip unloadable */ }
        }

        // Scan all loaded assemblies — now includes the force-loaded ones above.
        // Tests that provide an explicit plugin list can disable this to avoid
        // accidental duplicate registration from already-loaded test assemblies.
        if (configuration.GetValue($"{section}:IncludeLoadedAssemblies", true))
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                ScanAssembly(assembly, plugins, seen);
        }

        // 2. Explicit assembly paths from config — load & scan additional assemblies
        var assemblyPaths = configuration.GetSection($"{section}:Assemblies").Get<string[]>() ?? [];
        foreach (var path in assemblyPaths)
        {
            try { ScanAssembly(Assembly.LoadFrom(path), plugins, seen); }
            catch { /* skip unloadable */ }
        }

        // 3. Explicit type names from config — assembly-qualified, e.g.
        //    "Acme.MyPlugin, Acme.MyPlugin.Backend"
        var typeNames = configuration.GetSection($"{section}:Types").Get<string[]>() ?? [];
        foreach (var typeName in typeNames)
        {
            var type = Type.GetType(typeName);
            if (type is null || seen.Contains(type) || !typeof(T).IsAssignableFrom(type)) continue;
            try
            {
                plugins.Add((T)Activator.CreateInstance(type)!);
                seen.Add(type);
            }
            catch { /* skip uninstantiable */ }
        }

        // 4. NuGet package folder — scan all DLLs under the configured path
        var packagesPath = configuration[$"{section}:PackagesPath"];
        if (!string.IsNullOrEmpty(packagesPath) && Directory.Exists(packagesPath))
        {
            foreach (var dll in Directory.GetFiles(packagesPath, "*.dll", SearchOption.AllDirectories))
            {
                try { ScanAssembly(Assembly.LoadFrom(dll), plugins, seen); }
                catch { /* skip unloadable */ }
            }
        }

        return plugins;
    }

    [RequiresUnreferencedCode(
        "Plugin discovery loads plugin assemblies and instantiates plugin types by reflection, so the plugin types are not statically reachable and cannot be preserved by the trimmer.")]
    private static void ScanAssembly<T>(Assembly assembly, List<T> plugins, HashSet<Type> seen)
    {
        try
        {
            foreach (var type in assembly.GetExportedTypes())
            {
                if (!typeof(T).IsAssignableFrom(type) || type.IsAbstract || type.IsInterface)
                    continue;
                if (seen.Contains(type))
                    continue;
                if (type.GetConstructor(Type.EmptyTypes) is null)
                    continue;
                plugins.Add((T)Activator.CreateInstance(type)!);
                seen.Add(type);
            }
        }
        catch { /* skip assemblies that can't be reflected */ }
    }
}
