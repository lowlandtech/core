using System.ComponentModel;
using System.Diagnostics;

namespace LowlandTech.Core.Tests.Refinement;

/// <summary>
/// The runner of the repository owned gates of the Core lane. The gates are
/// Python scripts that the Core lane owns, and the tests that prove them read
/// the evidence that they print, so the process handling lives here instead
/// of in every test that needs it.
/// </summary>
public static class GateRunner
{
    /// <summary>Runs a Python gate and reads its result.</summary>
    public static (int ExitCode, string StandardOutput, string StandardError) Run(
        string gate, string repositoryRoot, params string[] arguments)
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
            foreach (var argument in arguments) start.ArgumentList.Add(argument);

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
    public static string? RepositoryRoot(string marker)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "eng", marker))
                && Directory.Exists(Path.Combine(directory.FullName, "src", "test")))
                return directory.FullName;
            directory = directory.Parent;
        }
        return null;
    }
}
