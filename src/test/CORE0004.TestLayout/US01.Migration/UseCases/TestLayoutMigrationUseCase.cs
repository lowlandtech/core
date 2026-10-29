using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Nodes;

namespace LowlandTech.Core.Tests.CORE0004.TestLayout.US01.Migration.UseCases;

/// <summary>
/// The Core owned measurement of the migration of the Core scenarios to the
/// shared agentic-gherkin layout.
///
/// The use case reads the repository with the Core owned layout gate
/// <c>eng/verify-test-layout.py</c>, proves that every scenario of the
/// repository follows the layout, proves that the gate rejects the negative
/// fixtures that carry one broken rule each, proves that the coverage that the
/// lane held before the migration is still mapped to its source criteria, and
/// proves that the canonical producer still builds trimmable and that the
/// isolated trimmed consumer of the published candidate still retains the
/// startup contract.
/// </summary>
public sealed class TestLayoutMigrationUseCase : global::LowlandTech.Core.Tests.Refinement.IUseCase
{
    private readonly List<global::LowlandTech.Core.Tests.Refinement.ResultCase> results = [];

    /// <summary>The named result cases of this scenario.</summary>
    public IReadOnlyList<global::LowlandTech.Core.Tests.Refinement.ResultCase> ResultCases => results;

    /// <summary>Every current test file of the lane is mapped to its source criteria.</summary>
    public bool? LegacyCoverageMapped { get; private set; }

    /// <summary>The scenarios of the lane follow the shared layout.</summary>
    public bool? ScenarioShapeProven { get; private set; }

    /// <summary>The layout gate rejects every broken layout that it is given.</summary>
    public bool? InvalidLayoutRejected { get; private set; }

    /// <summary>The coverage that the lane held before the migration is retained.</summary>
    public bool? ExecutedBehaviorRetained { get; private set; }

    /// <summary>The producer builds trimmable and the trimmed consumer is retained.</summary>
    public bool? TrimmedConsumerRetained { get; private set; }

    /// <summary>How often the fixture arranged this use case.</summary>
    public int ArrangementCount { get; private set; }

    /// <summary>How often the fixture invoked this use case.</summary>
    public int InvocationCount { get; private set; }

    /// <summary>The diagnostic that the measurement of a named result case recorded.</summary>
    public string Diagnostic(string key)
        => results.FirstOrDefault(result => result.Key == key)?.Diagnostic ?? "no diagnostic was recorded";

    /// <summary>The repository root of the Core lane.</summary>
    private string? RepositoryRoot { get; set; }

    public Task ArrangeAsync()
    {
        ArrangementCount++;
        RepositoryRoot = FindRepositoryRoot();
        return Task.CompletedTask;
    }

    public Task InvokeAsync()
    {
        InvocationCount++;
        Measure();
        return Task.CompletedTask;
    }

    private void Measure()
    {
        var root = RepositoryRoot;
        if (root is null)
        {
            Record("LegacyCoverageMapped", false, "The repository root was not found.");
            Record("ScenarioShapeProven", false, "The repository root was not found.");
            Record("InvalidLayoutRejected", false, "The repository root was not found.");
            Record("ExecutedBehaviorRetained", false, "The repository root was not found.");
            Record("TrimmedConsumerRetained", false, "The repository root was not found.");
            return;
        }

        var testRoot = Path.Combine(root, "src", "test");
        var evidence = RunGate(root, testRoot);
        var manifest = ReadManifest(root);

        LegacyCoverageMapped = MeasureLegacyCoverageMapped(evidence, manifest);
        ScenarioShapeProven = MeasureScenarioShape(evidence);
        InvalidLayoutRejected = MeasureInvalidLayoutRejected(root);
        ExecutedBehaviorRetained = MeasureExecutedBehaviorRetained(root, evidence, manifest);
        TrimmedConsumerRetained = MeasureTrimmedConsumerRetained(root);
    }

    /// <summary>Every current test file of the lane is mapped to a source criterion.</summary>
    private bool MeasureLegacyCoverageMapped(JsonObject evidence, JsonObject? manifest)
    {
        if (manifest is null)
        {
            Record("LegacyCoverageMapped", false, "The coverage baseline is missing.");
            return false;
        }
        var scanned = evidence["legacyTests"]!.AsArray()
            .ToDictionary(node => node!["file"]!.GetValue<string>(),
                             node => node!["facts"]!.GetValue<int>());
        var mapped = manifest["legacyTestFiles"]!.AsArray()
            .ToDictionary(node => node!["file"]!.GetValue<string>(), node => node!);

        var missing = scanned.Keys.Where(file => !mapped.ContainsKey(file)).OrderBy(file => file).ToList();
        var unmapped = mapped.Values
            .Where(entry => entry["criteria"]!.AsArray().Count == 0)
            .Select(entry => entry["file"]!.GetValue<string>())
            .OrderBy(file => file).ToList();
        var changed = mapped.Where(entry =>
        {
            var file = entry.Key;
            var facts = entry.Value["facts"]!.GetValue<int>();
            return !scanned.TryGetValue(file, out var current) || current != facts;
        }).Select(entry => entry.Key).OrderBy(file => file).ToList();

        var fixtures = manifest["backendParityFixtures"]!.AsArray().Count;
        var parityMapped = manifest["backendParityFixtures"]!.AsArray()
            .All(entry => entry!["criteria"]!.AsArray().Count > 0);

        // A family that was migrated into the layout carries its criteria with
        // it, so the coverage of a retired class is not lost.
        var scenarios = evidence["scenarios"]!.AsArray()
            .ToDictionary(node => node!["file"]!.GetValue<string>(),
                             node => node!["facts"]!.AsArray().Count);
        var migrated = manifest["migratedFamilies"]!.AsArray();
        var lostFamilies = migrated.Where(entry =>
        {
            var file = entry!["file"]!.GetValue<string>();
            return !scenarios.TryGetValue(file, out var facts) || facts != entry["facts"]!.GetValue<int>();
        }).Select(entry => entry!["file"]!.GetValue<string>()).OrderBy(file => file).ToList();
        var familiesWithoutCriteria = migrated
            .Where(entry => entry!["criteria"]!.AsArray().Count == 0)
            .Select(entry => entry!["file"]!.GetValue<string>())
            .OrderBy(file => file).ToList();

        var ok = missing.Count == 0 && unmapped.Count == 0 && changed.Count == 0
                 && lostFamilies.Count == 0 && familiesWithoutCriteria.Count == 0
                 && fixtures == 5 && parityMapped;
        Record("LegacyCoverageMapped", ok,
            ok
                ? $"{scanned.Count} current test files and {migrated.Count} migrated families are mapped "
                  + "to their source criteria, and the five Backend parity fixtures are mapped."
                : $"unmapped {string.Join(", ", missing)}; without criteria {string.Join(", ", unmapped)}; "
                  + $"changed {string.Join(", ", changed)}; lost families {string.Join(", ", lostFamilies)}; "
                  + $"families without criteria {string.Join(", ", familiesWithoutCriteria)}; "
                  + $"parity fixtures {fixtures}.");
        return ok;
    }

    /// <summary>The scenarios of the lane follow the shared layout.</summary>
    private bool MeasureScenarioShape(JsonObject evidence)
    {
        var violations = evidence["violations"]!.AsArray();
        var scenarios = evidence["scenarios"]!.AsArray();
        var shaped = scenarios.All(scenario =>
        {
            var entry = scenario!;
            var facts = entry["facts"]!.AsArray();
            var uacs = facts.Select(fact => fact!["uac"]!.GetValue<string>()).ToList();
            return entry["given"]!.GetValue<int>() == 1
                   && entry["when"]!.GetValue<int>() == 1
                   && facts.Count > 0
                   && facts.All(fact => fact!["expressionBodied"]!.GetValue<bool>()
                                        && fact["assertions"]!.GetValue<int>() == 1
                                        && !fact["skipped"]!.GetValue<bool>())
                   && uacs.Distinct().Count() == uacs.Count
                   && uacs.All(uac => !string.IsNullOrWhiteSpace(uac))
                   && entry["thenSteps"]!.AsArray().Count == facts.Count
                   && entry["lines"]!.GetValue<int>() <= 100;
        });

        // The fixture owns the invocation, so the arrangement and the invocation
        // run exactly once for the whole scenario.
        var once = ArrangementCount == 1 && InvocationCount == 1;
        var named = results.Count == 0 || results.Select(result => result.Key).Distinct().Count() == results.Count;
        var ok = violations.Count == 0 && scenarios.Count > 0 && shaped && once && named;
        Record("ScenarioShapeProven", ok,
            ok
                ? $"{scenarios.Count} scenarios hold one Given, one When and uniquely tagged "
                  + "expression-bodied Facts, and the fixture invoked the use case once."
                : $"{violations.Count} violations, {scenarios.Count} scenarios, arrangement "
                  + $"{ArrangementCount}, invocation {InvocationCount}.");
        return ok;
    }

    /// <summary>The layout gate rejects every broken layout that it is given.</summary>
    private bool MeasureInvalidLayoutRejected(string root)
    {
        var fixtureRoot = Path.Combine(root, "eng", "test-layout", "fixtures", "invalid");
        if (!Directory.Exists(fixtureRoot))
        {
            Record("InvalidLayoutRejected", false, "The negative fixtures are missing.");
            return false;
        }
        var rejected = 0;
        var missed = new List<string>();
        foreach (var fixture in Directory.GetDirectories(fixtureRoot).OrderBy(path => path))
        {
            var expected = Path.GetFileName(fixture);
            var evidence = RunGate(root, fixture);
            var codes = evidence["violations"]!.AsArray()
                .Select(node => node!["code"]!.GetValue<string>()).Distinct().ToList();
            if (codes.Contains(expected)) rejected++;
            else missed.Add($"{expected} ({string.Join(", ", codes)})");
        }
        var total = Directory.GetDirectories(fixtureRoot).Length;
        var ok = total > 0 && rejected == total;
        Record("InvalidLayoutRejected", ok,
            ok
                ? $"{rejected} of {total} negative fixtures were rejected with their own rule."
                : $"missed {string.Join("; ", missed)}.");
        return ok;
    }

    /// <summary>The coverage that the lane held before the migration is retained.</summary>
    private bool MeasureExecutedBehaviorRetained(string root, JsonObject evidence, JsonObject? manifest)
    {
        if (manifest is null)
        {
            Record("ExecutedBehaviorRetained", false, "The coverage baseline is missing.");
            return false;
        }
        var testRoot = Path.Combine(root, "src", "test");
        var lost = new List<string>();
        foreach (var entry in manifest["legacyTestFiles"]!.AsArray())
        {
            var file = entry!["file"]!.GetValue<string>();
            var path = Path.Combine(testRoot, file);
            if (!File.Exists(path))
            {
                lost.Add($"{file} is gone");
                continue;
            }
            var text = File.ReadAllText(path);
            var facts = Count(text, @"\bFact\b(?=\s*[\(\],])");
            var theories = Count(text, @"\bTheory\b(?=\s*[\(\],])");
            var cases = Count(text, @"\bInlineData\b");
            if (facts != entry["facts"]!.GetValue<int>()
                || theories != entry["theories"]!.GetValue<int>()
                || cases != entry["inlineData"]!.GetValue<int>())
            {
                lost.Add($"{file} changed its cases");
            }
        }

        var changedFixtures = new List<string>();
        foreach (var entry in manifest["backendParityFixtures"]!.AsArray())
        {
            var file = entry!["file"]!.GetValue<string>();
            var path = Path.Combine(testRoot, file);
            if (!File.Exists(path))
            {
                changedFixtures.Add($"{file} is gone");
                continue;
            }
            var hash = global::LowlandTech.Core.Tests.Refinement.PackageFeed.Sha256(path);
            if (hash != entry["sha256"]!.GetValue<string>()) changedFixtures.Add($"{file} changed");
        }

        var scanned = evidence["legacyTests"]!.AsArray().Count;
        var scenarios = evidence["scenarios"]!.AsArray()
            .ToDictionary(node => node!["file"]!.GetValue<string>(),
                             node => node!["facts"]!.AsArray().Count);
        var lostMigrations = manifest["migratedFamilies"]!.AsArray()
            .Where(entry =>
            {
                var file = entry!["file"]!.GetValue<string>();
                return !scenarios.TryGetValue(file, out var facts) || facts != entry["facts"]!.GetValue<int>();
            })
            .Select(entry => entry!["file"]!.GetValue<string>()).ToList();
        var ok = lost.Count == 0 && changedFixtures.Count == 0 && lostMigrations.Count == 0 && scanned > 0;
        Record("ExecutedBehaviorRetained", ok,
            ok
                ? $"{scanned} legacy test files keep their exact cases, the migrated families execute "
                  + "their own facts, and the five Backend parity fixtures keep their hashes."
                : $"lost {string.Join("; ", lost)}; fixtures {string.Join("; ", changedFixtures)}; "
                  + $"lost migrations {string.Join("; ", lostMigrations)}.");
        return ok;
    }

    /// <summary>The producer builds trimmable and the trimmed consumer is retained.</summary>
    private bool MeasureTrimmedConsumerRetained(string root)
    {
        var build = RunProcess("dotnet", [
            "build", Path.Combine(root, "src", "lib", "LowlandTech.Core.csproj"),
            "-c", "Release", "-m:1", "-p:EnableTrimAnalyzer=true"
        ], root);
        var warnings = Count(build.Output, "warning IL");
        if (build.ExitCode != 0 || warnings != 0)
        {
            Record("TrimmedConsumerRetained", false,
                $"The release build reported exit {build.ExitCode} with {warnings} trim warnings.");
            return false;
        }

        var feed = Path.Combine(Directory.GetParent(root)!.FullName, "packages");
        var candidate = global::LowlandTech.Core.Tests.Refinement.PackageFeed.NewestCoreCandidate(feed);
        if (candidate is null)
        {
            Record("TrimmedConsumerRetained", false, "The feed carries no published Core candidate.");
            return false;
        }
        var version = Path.GetFileName(candidate)[$"LowlandTech.Core.".Length..^".nupkg".Length];
        var declared = global::LowlandTech.Core.Tests.Refinement.PackageFeed.DeclaredDependency(
            candidate, "LowlandTech.Components.Core");
        if (declared is null || !declared.StartsWith('[') || !declared.EndsWith(']'))
        {
            Record("TrimmedConsumerRetained", false,
                $"The published candidate does not declare an exact dependency: {declared}.");
            return false;
        }

        var consumer = Path.Combine(Path.GetTempPath(),
            $"lowlandtech-core-layout-consumer-{Guid.NewGuid():N}");
        Directory.CreateDirectory(consumer);
        try
        {
            foreach (var file in new[] { "Consumer.csproj", "Program.cs" })
            {
                File.Copy(Path.Combine(root, "src", "consumer", file), Path.Combine(consumer, file), true);
            }
            var config = Path.Combine(consumer, "NuGet.Config");
            File.WriteAllText(config,
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><configuration><packageSources><clear/>"
                + $"<add key=\"feed\" value=\"{feed}\"/>"
                + "<add key=\"nuget.org\" value=\"https://api.nuget.org/v3/index.json\"/>"
                + "</packageSources></configuration>");
            var project = Path.Combine(consumer, "Consumer.csproj");
            var restore = RunProcess("dotnet", [
                "restore", project, "--configfile", config, "-p:CorePackageVersion=" + version
            ], consumer);
            if (restore.ExitCode != 0)
            {
                Record("TrimmedConsumerRetained", false, $"The consumer restore failed: {restore.Output}");
                return false;
            }
            var rid = System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;
            var trimmed = Path.Combine(consumer, "trimmed");
            var publish = RunProcess("dotnet", [
                "publish", project, "-c", "Release", "--no-restore", "-r", rid,
                "--self-contained", "true", "-p:CorePackageVersion=" + version, "-o", trimmed
            ], consumer);
            var trimWarnings = Count(publish.Output, "Trim analysis warning IL");
            if (publish.ExitCode != 0 || trimWarnings != 0)
            {
                Record("TrimmedConsumerRetained", false,
                    $"The trimmed publish reported exit {publish.ExitCode} with {trimWarnings} warnings.");
                return false;
            }
            var executable = Path.Combine(trimmed, OperatingSystem.IsWindows() ? "Consumer.exe" : "Consumer");
            var run = RunProcess(executable, [], consumer);
            var report = run.Output;
            var retained = report.Contains("\"startupContract\":\"LowlandTech.Components.Core.ComponentsStartupMap\"")
                           && report.Contains("\"registryKey\":\"header\"")
                           && report.Contains("\"installModes\":\"Routed\"");
            var ok = run.ExitCode == 0 && retained;
            Record("TrimmedConsumerRetained", ok,
                ok
                    ? $"The published candidate {version} builds with zero trim warnings and the trimmed "
                      + "consumer retains the startup contract."
                    : $"The trimmed consumer reported exit {run.ExitCode}: {report}");
            return ok;
        }
        finally
        {
            try { Directory.Delete(consumer, true); } catch (IOException) { /* the temporary proof is left behind */ }
        }
    }

    private void Record(string key, bool outcome, string diagnostic)
    {
        results.RemoveAll(result => result.Key == key);
        results.Add(new global::LowlandTech.Core.Tests.Refinement.ResultCase(
            key, outcome ? "Observed" : "NotObserved", diagnostic));
        switch (key)
        {
            case "LegacyCoverageMapped": LegacyCoverageMapped = outcome; break;
            case "ScenarioShapeProven": ScenarioShapeProven = outcome; break;
            case "InvalidLayoutRejected": InvalidLayoutRejected = outcome; break;
            case "ExecutedBehaviorRetained": ExecutedBehaviorRetained = outcome; break;
            case "TrimmedConsumerRetained": TrimmedConsumerRetained = outcome; break;
        }
    }

    /// <summary>Runs the layout gate over a root and reads its evidence.</summary>
    private static JsonObject RunGate(string repositoryRoot, string root)
    {
        var gate = Path.Combine(repositoryRoot, "eng", "verify-test-layout.py");
        var run = RunProcess("python", [gate, "--root", root, "--json"], repositoryRoot);
        if (string.IsNullOrWhiteSpace(run.Output))
        {
            return new JsonObject { ["violations"] = new JsonArray(), ["scenarios"] = new JsonArray(),
                                    ["legacyTests"] = new JsonArray() };
        }
        try
        {
            return JsonNode.Parse(run.Output)!.AsObject();
        }
        catch (System.Text.Json.JsonException)
        {
            return new JsonObject { ["violations"] = new JsonArray(), ["scenarios"] = new JsonArray(),
                                    ["legacyTests"] = new JsonArray() };
        }
    }

    /// <summary>The coverage baseline of the migration.</summary>
    private static JsonObject? ReadManifest(string repositoryRoot)
    {
        var path = Path.Combine(repositoryRoot, "src", "test", "CORE0004.TestLayout", "US01.Migration",
            "inventory", "legacy-coverage.json");
        if (!File.Exists(path)) return null;
        return JsonNode.Parse(File.ReadAllText(path))!.AsObject();
    }

    /// <summary>Counts the matches of a pattern in a text.</summary>
    private static int Count(string text, string pattern)
        => System.Text.RegularExpressions.Regex.Matches(text, pattern).Count;

    /// <summary>Runs a process and reads its output.</summary>
    private static (int ExitCode, string Output) RunProcess(string file, string[] arguments, string workingDirectory)
    {
        var start = new ProcessStartInfo(file)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        try
        {
            using var process = Process.Start(start);
            if (process is null) return (-1, "The process could not be started.");
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit((int)TimeSpan.FromMinutes(20).TotalMilliseconds))
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit();
            }
            return (process.ExitCode, output.GetAwaiter().GetResult() + error.GetAwaiter().GetResult());
        }
        catch (Win32Exception exception)
        {
            return (-1, exception.Message);
        }
    }

    /// <summary>The root of the Core repository the test runs from.</summary>
    private static string? FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "eng", "verify-test-layout.py"))
                && Directory.Exists(Path.Combine(directory.FullName, "src", "test")))
                return directory.FullName;
            directory = directory.Parent;
        }
        return null;
    }
}
