using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace LowlandTech.Core.Tests;

/// <summary>
/// The Core owned half of UAC02 of CORE0001-US02-SC01: the canonical producer
/// has to pin the generation of ``LowlandTech.Components.Core`` that carries the
/// startup contract, and the published candidate has to declare that same pin.
///
/// The producer pins an exact version, and the isolated consumer of the package
/// resolves exactly that version. A consumer that also references the
/// dependency directly resolves the highest version the feed offers instead,
/// and NuGet compares the versions of this workspace by their numeric parts.
/// The measurement of that resolution is the repository owned gate at
/// eng/verify-closure.py.
///
/// The gate reports a divergence with exit code 2. That is a finding for the
/// lane that owns the dependency, not a Core owned failure, so this Fact
/// requires the Core owned invariants and records the divergence with its
/// owner.
/// </summary>
public sealed class PackageClosureAcceptanceTests
{
    /// <summary>The dependency that the canonical producer declares exactly.</summary>
    private const string DependencyId = "LowlandTech.Components.Core";

    /// <summary>The package identity that the canonical producer publishes.</summary>
    private const string CorePackageId = "LowlandTech.Core";

    /// <summary>
    /// The types of the dependency that the trimmed consumer of the producer
    /// builds against, so a generation without them cannot satisfy the
    /// producer's own contract.
    /// </summary>
    private static readonly string[] StartupContractTypes =
    {
        "LowlandTech.Components.Core.ComponentsStartupMap",
        "LowlandTech.Components.Core.ComponentsCoreStartup",
        "LowlandTech.Components.Core.ComponentsStartupBinding"
    };

    [Fact]
    public void UAC02_TheCanonicalCoreClosurePinsTheStartupContractGeneration()
    {
        var repositoryRoot = RepositoryRoot();
        repositoryRoot.ShouldNotBeNull("The Core repository root was not found from the test output directory.");

        var gate = Path.Combine(repositoryRoot!, "eng", "verify-closure.py");
        File.Exists(gate).ShouldBeTrue($"The closure gate '{gate}' is missing from the repository.");

        var workspaceRoot = Directory.GetParent(repositoryRoot!)!.FullName;
        var feedRoot = Path.Combine(workspaceRoot, "packages");
        var run = RunGate(gate, repositoryRoot!, workspaceRoot);

        // Exit code 0 means the pinned generation wins the resolution and exit
        // code 2 means the Core owned invariants hold while another generation
        // of the dependency wins it. Exit code 1 is a Core owned violation and
        // exit code 3 means the measurement did not run at all.
        (run.ExitCode is 0 or 2).ShouldBeTrue(
            $"The closure gate reported exit code {run.ExitCode}. {run.StandardError}{run.StandardOutput}");

        var evidence = JsonNode.Parse(run.StandardOutput)!.AsObject();

        // The producer declares an exact dependency and the measurement found no
        // Core owned violation of it.
        var pinned = evidence["pinned"]!.GetValue<string>();
        pinned.ShouldNotBeNullOrWhiteSpace("The producer does not declare a dependency to pin.");
        evidence["pinnedIsExact"]!.GetValue<bool>().ShouldBeTrue(
            "The producer does not declare an exact dependency on the Components.Core package.");
        evidence["coreViolations"]!.AsArray().Count.ShouldBe(0);
        evidence["pinnedInFeed"]!.GetValue<bool>().ShouldBeTrue(
            $"The pinned generation {pinned} is not in the local feed.");

        // The pinned generation has to be unambiguous: a direct reference
        // resolves it, and no version of the feed sorts above it, so a
        // publisher that resolves the highest version of the feed takes it too.
        var available = evidence["availableVersions"]!.AsArray()
            .Select(node => node!.GetValue<string>()).ToList();
        available.ShouldNotBeEmpty("The local feed offers no version of the dependency.");
        evidence["directResolution"]!.GetValue<string>().ShouldBe(pinned);
        evidence["highestAvailable"]!.GetValue<string>().ShouldBe(Highest(available));
        evidence["pinIsUnambiguous"]!.GetValue<bool>().ShouldBeTrue(
            $"The pinned generation {pinned} is not the only generation that the feed can resolve.");

        // A version that sorts above the pin is a finding for the lane that
        // owns the dependency, so it is named with its owner.
        var shadowing = evidence["shadowingVersions"]!.AsArray()
            .Select(node => node!.GetValue<string>()).ToList();
        if (shadowing.Count > 0)
        {
            var reported = evidence["externalHazards"]!.AsArray()
                .Any(node => node!["kind"]!.GetValue<string>() == "shadowing-versions");
            reported.ShouldBeTrue(
                "The feed holds versions above the pinned generation and the gate did not report them.");
        }

        // A divergence is a finding for the lane that owns the dependency, so it
        // is named with its owner instead of being reported as a Core failure.
        foreach (var hazard in evidence["externalHazards"]!.AsArray())
        {
            hazard!["kind"]!.GetValue<string>().ShouldBeOneOf("shadowing-versions", "batch-closure-divergence");
            hazard["owner"]!.GetValue<string>().ShouldBe(evidence["dependencyOwner"]!.GetValue<string>());
            hazard["owner"]!.GetValue<string>().ShouldNotBe("core");
            hazard["pinned"]!.GetValue<string>().ShouldBe(pinned);
        }

        // The batch that the Components lane publishes is what a consumer of the
        // workspace takes, so the gate measures the closure it produces. When
        // that closure resolves another generation, the divergence is reported.
        var consumer = evidence["consumerBatch"];
        consumer.ShouldNotBeNull("The gate did not measure the published batch of the Components lane.");
        consumer!["package"]!.GetValue<string>().ShouldBe("LowlandTech.Components");
        consumer["declaredDependency"]!.GetValue<string>().ShouldNotBeNullOrWhiteSpace();
        if (!consumer["resolvesToPinnedGeneration"]!.GetValue<bool>())
        {
            var reported = evidence["externalHazards"]!.AsArray()
                .Any(node => node!["kind"]!.GetValue<string>() == "batch-closure-divergence");
            reported.ShouldBeTrue(
                "The published batch resolves another generation of the dependency and the "
                + "gate did not report the divergence.");
        }

        // The pinned generation carries the startup contract that the trimmed
        // consumer of the producer builds against.
        var pinnedPackage = Path.Combine(feedRoot, $"{DependencyId}.{pinned}.nupkg");
        File.Exists(pinnedPackage).ShouldBeTrue($"The pinned package '{pinnedPackage}' is missing from the feed.");
        var pinnedTypes = PublicTypes(pinnedPackage);
        foreach (var expected in StartupContractTypes)
        {
            pinnedTypes.ShouldContain(expected,
                $"The pinned generation {pinned} does not carry '{expected}'.");
        }

        // The published candidate declares an exact dependency that the feed
        // carries. It names the same generation as the source of the producer
        // once that generation is published, and the generation before it while
        // the publication is still pending. A candidate that declares another
        // generation is a drift of the producer.
        var candidate = HighestCoreCandidate(feedRoot);
        candidate.ShouldNotBeNull("The local feed carries no published Core candidate.");
        var declared = DeclaredDependency(candidate!, DependencyId);
        declared.ShouldNotBeNullOrWhiteSpace(
            "The published Core candidate does not declare a dependency on the Components.Core package.");
        var declaredGeneration = declared!.Trim('[', ']');
        available.ShouldContain(declaredGeneration,
            $"The published Core candidate declares {declaredGeneration}, which the feed does not carry.");
        ComparePublication(declaredGeneration, pinned).ShouldBeLessThanOrEqualTo(0,
            $"The published Core candidate declares {declaredGeneration} while the producer "
            + $"declares {pinned}; the published package is newer than the source of the producer.");
    }

    /// <summary>The highest of the given versions, in the NuGet order of this feed.</summary>
    private static string Highest(IEnumerable<string> versions)
    {
        var highest = versions.First();
        foreach (var candidate in versions.Skip(1))
        {
            if (Compare(candidate, highest) > 0) highest = candidate;
        }
        return highest;
    }

    /// <summary>The newest of the given versions, in the order they were published.</summary>
    private static string HighestPublished(IEnumerable<string> versions)
    {
        var newest = versions.First();
        foreach (var candidate in versions.Skip(1))
        {
            if (ComparePublication(candidate, newest) > 0) newest = candidate;
        }
        return newest;
    }

    /// <summary>
    /// The order in which the versions of this feed were published. The stamp
    /// carries the publication date and the time behind it, and the release
    /// train number of the version does not decide that order.
    /// </summary>
    private static int ComparePublication(string left, string right)
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

    /// <summary>
    /// The order that NuGet applies to the versions of this feed: the numeric
    /// parts decide first, so a higher patch number wins over the release label,
    /// and the release label only separates two versions that share them.
    /// </summary>
    private static int Compare(string left, string right)
    {
        var first = Parts(left);
        var second = Parts(right);
        var numeric = first.Major.CompareTo(second.Major);
        if (numeric != 0) return numeric;
        numeric = first.Minor.CompareTo(second.Minor);
        if (numeric != 0) return numeric;
        numeric = first.Patch.CompareTo(second.Patch);
        if (numeric != 0) return numeric;
        if (first.Release.Length == 0 && second.Release.Length == 0) return 0;
        var firstIsNumeric = first.Release.All(char.IsDigit);
        var secondIsNumeric = second.Release.All(char.IsDigit);
        if (firstIsNumeric && secondIsNumeric)
        {
            // The stamps of this feed are longer than a 64 bit number, so they
            // are compared as digits instead of being parsed.
            if (first.Release.Length != second.Release.Length)
                return first.Release.Length.CompareTo(second.Release.Length);
            return string.CompareOrdinal(first.Release, second.Release);
        }
        if (firstIsNumeric) return 1;
        if (secondIsNumeric) return -1;
        return string.CompareOrdinal(first.Release, second.Release);
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

    /// <summary>The newest published Core candidate in the local feed.</summary>
    private static string? HighestCoreCandidate(string feedRoot)
    {
        if (!Directory.Exists(feedRoot)) return null;
        var prefix = CorePackageId + ".";
        var candidates = Directory.GetFiles(feedRoot, $"{CorePackageId}.*.nupkg")
            .Select(path => (Path: path, Version: Path.GetFileName(path)[prefix.Length..^".nupkg".Length]))
            .ToList();
        if (candidates.Count == 0) return null;
        var highest = candidates[0];
        foreach (var candidate in candidates.Skip(1))
        {
            if (ComparePublication(candidate.Version, highest.Version) > 0) highest = candidate;
        }
        return highest.Path;
    }

    /// <summary>The public types that a package publishes.</summary>
    private static IReadOnlyList<string> PublicTypes(string packagePath)
    {
        using var archive = ZipFile.OpenRead(packagePath);
        var entry = archive.Entries.FirstOrDefault(candidate =>
            candidate.FullName.StartsWith("lib/", StringComparison.Ordinal)
            && candidate.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase));
        entry.ShouldNotBeNull($"The package '{packagePath}' publishes no library.");

        using var buffer = new MemoryStream();
        using (var entryStream = entry!.Open()) entryStream.CopyTo(buffer);
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
    private static string? DeclaredDependency(string packagePath, string dependencyId)
    {
        using var archive = ZipFile.OpenRead(packagePath);
        var entry = archive.Entries.FirstOrDefault(candidate =>
            candidate.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
        entry.ShouldNotBeNull($"The package '{packagePath}' carries no nuspec.");

        using var stream = entry!.Open();
        var document = XDocument.Load(stream);
        var dependency = document.Descendants()
            .FirstOrDefault(element => element.Name.LocalName == "dependency"
                                       && (string?)element.Attribute("id") == dependencyId);
        return (string?)dependency?.Attribute("version");
    }

    /// <summary>Runs the closure gate and reads its evidence.</summary>
    private static (int ExitCode, string StandardOutput, string StandardError) RunGate(
        string gate, string repositoryRoot, string workspaceRoot)
    {
        foreach (var interpreter in new[] { ("python", Array.Empty<string>()), ("py", new[] { "-3" }) })
        {
            var start = new ProcessStartInfo(interpreter.Item1)
            {
                WorkingDirectory = repositoryRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            foreach (var argument in interpreter.Item2) start.ArgumentList.Add(argument);
            start.ArgumentList.Add(gate);
            start.ArgumentList.Add("--repository-root");
            start.ArgumentList.Add(repositoryRoot);
            start.ArgumentList.Add("--workspace-root");
            start.ArgumentList.Add(workspaceRoot);
            start.ArgumentList.Add("--json");

            try
            {
                using var process = Process.Start(start);
                if (process is null) continue;
                var output = process.StandardOutput.ReadToEndAsync();
                var error = process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit((int)TimeSpan.FromMinutes(15).TotalMilliseconds))
                {
                    process.Kill(entireProcessTree: true);
                    process.WaitForExit();
                }
                return (process.ExitCode, output.GetAwaiter().GetResult(), error.GetAwaiter().GetResult());
            }
            catch (Win32Exception)
            {
                // The interpreter is not on this machine, so the next candidate is tried.
            }
        }
        throw new InvalidOperationException(
            "Neither python nor py could be started. The Core verification tooling requires a Python interpreter.");
    }

    /// <summary>The root of the Core repository the test runs from.</summary>
    private static string? RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "eng", "verify-closure.py"))
                && Directory.Exists(Path.Combine(directory.FullName, "src", "test")))
                return directory.FullName;
            directory = directory.Parent;
        }
        return null;
    }
}
