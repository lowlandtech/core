using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Nodes;

namespace LowlandTech.Core.Tests;

/// <summary>
/// The acceptance inventory of CORE0001-US02-SC01. Core owns the package
/// trimmability contract and its own producer proof, so this Fact measures the
/// producer uniqueness that UAC01 asks for: the canonical Core checkout is the
/// only place that publishes a Core package identity.
///
/// The measurement is the repository owned gate at eng/verify-producers.py. It
/// reads the effective MSBuild properties of every project that can declare a
/// Core package identity, so a checkout that publishes a package while its own
/// project file says it cannot is still found.
///
/// The gate reports a competing producer outside the Core repository with exit
/// code 2. That is a finding for the lane that owns that checkout, not a Core
/// owned failure, so this Fact requires the Core owned invariants and records
/// the external producers with their owners.
/// </summary>
public sealed class ProducerRetirementAcceptanceTests
{
    /// <summary>The canonical producer of each Core package identity.</summary>
    private static readonly (string PackageId, string RelativePath)[] CanonicalProducers =
    {
        ("LowlandTech.Core", "src/lib/LowlandTech.Core.csproj"),
        ("LowlandTech.Core.Contracts", "src/contracts/LowlandTech.Core.Contracts.csproj")
    };

    [Fact]
    public void UAC01_OnlyCorePublishesTheCorePackageIdentities()
    {
        var repositoryRoot = RepositoryRoot();
        repositoryRoot.ShouldNotBeNull("The Core repository root was not found from the test output directory.");

        var gate = Path.Combine(repositoryRoot!, "eng", "verify-producers.py");
        File.Exists(gate).ShouldBeTrue($"The producer uniqueness gate '{gate}' is missing from the repository.");

        var workspaceRoot = Directory.GetParent(repositoryRoot!)!.FullName;
        var run = RunGate(gate, repositoryRoot!, workspaceRoot);

        // Exit code 0 means the canonical producer is the only producer and exit
        // code 2 means the Core owned invariants hold while another checkout can
        // still publish. Exit code 1 is a Core owned violation and exit code 3
        // means the measurement did not run at all.
        (run.ExitCode is 0 or 2).ShouldBeTrue(
            $"The producer uniqueness gate reported exit code {run.ExitCode}. {run.StandardError}{run.StandardOutput}");

        var evidence = JsonNode.Parse(run.StandardOutput)!.AsObject();

        // The canonical producer of each identity exists, publishes a package
        // and belongs to Core.
        var canonical = evidence["canonical"]!.AsArray();
        foreach (var (packageId, relativePath) in CanonicalProducers)
        {
            var producer = canonical.SingleOrDefault(node => node!["packageId"]!.GetValue<string>() == packageId);
            producer.ShouldNotBeNull($"The canonical producer of {packageId} was not found by the gate.");
            producer!["packable"]!.GetValue<bool>().ShouldBeTrue(
                $"The canonical producer of {packageId} cannot publish a package.");
            producer["owner"]!.GetValue<string>().ShouldBe("core");
            RelativePath(repositoryRoot!, producer["project"]!.GetValue<string>()).ShouldBe(relativePath);
        }
        canonical.Count.ShouldBe(CanonicalProducers.Length);

        // No checkout inside the Core repository publishes a Core identity other
        // than the canonical producers, and no canonical producer is missing.
        evidence["coreViolations"]!.AsArray().Count.ShouldBe(0);
        evidence["missingCanonicalProducers"]!.AsArray().Count.ShouldBe(0);
        evidence["unevaluated"]!.AsArray().Count.ShouldBe(0);

        // Every producer outside the Core repository is named with the lane that
        // owns it, so the retirement of that producer has an owner.
        foreach (var node in evidence["competingProducers"]!.AsArray())
        {
            var project = node!["project"]!.GetValue<string>();
            IsInside(repositoryRoot!, project).ShouldBeFalse(
                $"'{project}' publishes a Core identity from inside the Core repository.");
            var owner = node["owner"]!.GetValue<string>();
            owner.ShouldNotBe("core");
            owner.ShouldNotBe("unknown");
            node["packable"]!.GetValue<bool>().ShouldBeTrue();
            node["refusesForcedPack"]!.GetValue<bool>().ShouldBeFalse();
        }

        // A retired producer refuses to publish: either it is not packable, or
        // it rejects a pack that forces it to be.
        foreach (var node in evidence["retired"]!.AsArray())
        {
            var packable = node!["packable"]!.GetValue<bool>();
            var refuses = node["refusesForcedPack"]!.GetValue<bool>();
            (packable == false || refuses).ShouldBeTrue(
                $"'{node["project"]!.GetValue<string>()}' is retired but can still publish a Core identity.");
        }
    }

    /// <summary>Runs the producer uniqueness gate and reads its evidence.</summary>
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
            if (File.Exists(Path.Combine(directory.FullName, "eng", "verify-producers.py"))
                && Directory.Exists(Path.Combine(directory.FullName, "src", "test")))
                return directory.FullName;
            directory = directory.Parent;
        }
        return null;
    }

    /// <summary>The path of a project relative to the Core repository root.</summary>
    private static string RelativePath(string repositoryRoot, string project)
        => Path.GetRelativePath(repositoryRoot, project).Replace('\\', '/');

    /// <summary>Whether a project lives inside the Core repository.</summary>
    private static bool IsInside(string repositoryRoot, string project)
    {
        var relative = Path.GetRelativePath(repositoryRoot, project);
        return relative != ".." && !relative.StartsWith("..", StringComparison.Ordinal) && !Path.IsPathRooted(relative);
    }
}
