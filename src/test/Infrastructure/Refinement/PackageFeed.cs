using System.IO.Compression;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;

namespace LowlandTech.Core.Tests.Refinement;

/// <summary>
/// The packages of the local feed and the order of their versions. The Core
/// tests read the feed to prove the closure of the producer, so the reading of
/// a package and the ordering of the versions live here instead of in every
/// test that needs them.
/// </summary>
public static class PackageFeed
{
    /// <summary>The identity of the canonical producer of the Core lane.</summary>
    public const string CorePackageId = "LowlandTech.Core";

    /// <summary>The newest published candidate of the canonical producer.</summary>
    public static string? NewestCoreCandidate(string feedRoot)
    {
        if (!Directory.Exists(feedRoot)) return null;
        var candidates = Directory.GetFiles(feedRoot, $"{CorePackageId}.*.nupkg")
            .Select(path => (Path: path, Version: Path.GetFileName(path)[$"{CorePackageId}.".Length..^".nupkg".Length]))
            .Where(entry => entry.Version.Length > 0 && char.IsDigit(entry.Version[0]))
            .ToList();
        if (candidates.Count == 0) return null;
        var newest = candidates[0];
        foreach (var candidate in candidates.Skip(1))
        {
            if (ComparePublication(candidate.Version, newest.Version) > 0) newest = candidate;
        }
        return newest.Path;
    }

    /// <summary>The public types that a package publishes.</summary>
    public static IReadOnlyList<string> PublicTypes(string packagePath)
    {
        using var archive = ZipFile.OpenRead(packagePath);
        var entry = archive.Entries.FirstOrDefault(candidate =>
            candidate.FullName.StartsWith("lib/", StringComparison.Ordinal)
            && candidate.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase));
        if (entry is null) return [];

        using var buffer = new MemoryStream();
        using (var entryStream = entry.Open()) entryStream.CopyTo(buffer);
        buffer.Position = 0;
        using var peReader = new PEReader(buffer);
        var reader = peReader.GetMetadataReader();
        var names = new List<string>();
        foreach (var handle in reader.TypeDefinitions)
        {
            var definition = reader.GetTypeDefinition(handle);
            var visibility = definition.Attributes & TypeAttributes.VisibilityMask;
            if (visibility != TypeAttributes.Public && visibility != TypeAttributes.NestedPublic) continue;
            var space = reader.GetString(definition.Namespace);
            var name = reader.GetString(definition.Name);
            names.Add(space.Length == 0 ? name : $"{space}.{name}");
        }
        return names;
    }

    /// <summary>The version that a published package declares for a dependency.</summary>
    public static string? DeclaredDependency(string packagePath, string dependencyId)
    {
        using var archive = ZipFile.OpenRead(packagePath);
        var entry = archive.Entries.FirstOrDefault(candidate =>
            candidate.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
        if (entry is null) return null;
        using var stream = entry.Open();
        var document = System.Xml.Linq.XDocument.Load(stream);
        var dependency = document.Descendants()
            .FirstOrDefault(element => element.Name.LocalName == "dependency"
                                       && (string?)element.Attribute("id") == dependencyId);
        return (string?)dependency?.Attribute("version");
    }

    /// <summary>The hash of a file.</summary>
    public static string Sha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    /// <summary>The highest of the given versions, in the NuGet order of this feed.</summary>
    public static string Highest(IEnumerable<string> versions)
    {
        var highest = versions.First();
        foreach (var candidate in versions.Skip(1))
        {
            if (Compare(candidate, highest) > 0) highest = candidate;
        }
        return highest;
    }

    /// <summary>
    /// The order in which NuGet compares the versions of this feed. The numeric
    /// parts decide first, so a higher patch number wins over the release label.
    /// </summary>
    public static int Compare(string left, string right)
    {
        var first = Parts(left);
        var second = Parts(right);
        var numeric = first.Major.CompareTo(second.Major);
        if (numeric != 0) return numeric;
        numeric = first.Minor.CompareTo(second.Minor);
        if (numeric != 0) return numeric;
        numeric = first.Patch.CompareTo(second.Patch);
        if (numeric != 0) return numeric;
        var firstIsNumeric = first.Release.Length > 0 && first.Release.All(char.IsDigit);
        var secondIsNumeric = second.Release.Length > 0 && second.Release.All(char.IsDigit);
        if (firstIsNumeric && secondIsNumeric)
        {
            if (first.Release.Length != second.Release.Length)
                return first.Release.Length.CompareTo(second.Release.Length);
            return string.CompareOrdinal(first.Release, second.Release);
        }
        if (firstIsNumeric) return 1;
        if (secondIsNumeric) return -1;
        return string.CompareOrdinal(first.Release, second.Release);
    }

    /// <summary>
    /// The order in which the versions of this feed were published. The stamp
    /// carries the publication date and the time behind it, and the release
    /// train number of the version does not decide that order.
    /// </summary>
    public static int ComparePublication(string left, string right)
    {
        var first = Parts(left);
        var second = Parts(right);
        var numeric = Stamp(first.Release).CompareTo(Stamp(second.Release));
        if (numeric != 0) return numeric;
        return string.CompareOrdinal(first.Release, second.Release);
    }

    /// <summary>The publication date and time that the stamp of a version carries.</summary>
    private static (int Date, int Time) Stamp(string release)
    {
        if (release.Length == 0 || !release.All(char.IsDigit)) return (0, 0);
        var date = release.Length >= 8 ? int.Parse(release[..8]) : 0;
        var time = release.Length >= 14 ? int.Parse(release.Substring(8, 6)) : 0;
        return (date, time);
    }

    /// <summary>The numeric parts and the release label of a version.</summary>
    private static (int Major, int Minor, int Patch, string Release) Parts(string version)
    {
        var numeric = version.Split('-')[0].Split('.');
        var major = int.TryParse(numeric.ElementAtOrDefault(0), out var first) ? first : 0;
        var minor = int.TryParse(numeric.ElementAtOrDefault(1), out var second) ? second : 0;
        var patch = int.TryParse(numeric.ElementAtOrDefault(2), out var third) ? third : 0;
        var release = version.Contains('-') ? version[(version.IndexOf('-') + 1)..] : string.Empty;
        return (major, minor, patch, release);
    }
}
