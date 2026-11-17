using System;
using System.IO;

namespace fxpa.benchmarks;

/// <summary>
/// Locates repository assets (the bundled GBPUSD .cdl fixture) for both the
/// host process and the child processes BenchmarkDotNet spawns.
///
/// BenchmarkDotNet runs each benchmark in a freshly generated child project
/// whose output directory lives under the artifacts folder, so the child
/// cannot rely on the working directory of the shell that started the run.
/// The parent therefore resolves the fixture once and hands the absolute path
/// down through an environment variable, which the children inherit.
/// </summary>
internal static class RepoPaths
{
    /// <summary>Environment variable carrying the absolute fixture path to child processes.</summary>
    internal const string FixtureEnvironmentVariable = "FXPA_CDL_FIXTURE";

    private const string SolutionFileName = "fx-trading-dotnet.sln";

    private static readonly string[] FixtureSegments =
    {
        "src", "fxpa", "fxd", "GBPUSD", "MIN1", "local", "GBPUSD.cdl"
    };

    /// <summary>Absolute path of the bundled 5,320 line GBPUSD MIN1 fixture.</summary>
    internal static string FixturePath => ResolveFixturePath();

    /// <summary>
    /// Resolves the fixture in the parent process and exports it so that the
    /// spawned benchmark processes find the very same file.
    /// </summary>
    internal static void PublishFixturePathToChildProcesses()
    {
        try
        {
            Environment.SetEnvironmentVariable(FixtureEnvironmentVariable, ResolveFixturePath());
        }
        catch (FileNotFoundException)
        {
            // Not fatal for the rest of the suite - only the data load
            // benchmark needs the fixture, and it reports the problem itself.
        }
    }

    private static string ResolveFixturePath()
    {
        // NB: the fxpa assembly ships its own fxpa.AppContext class, which
        // shadows System.AppContext inside the fxpa.benchmarks namespace.
        // Always spell the framework one out in full.
        string? fromEnvironment = Environment.GetEnvironmentVariable(FixtureEnvironmentVariable);
        if (!string.IsNullOrEmpty(fromEnvironment) && File.Exists(fromEnvironment))
        {
            return fromEnvironment;
        }

        // Preferred: the canonical file inside the repository.
        string? repoRoot = FindRepoRoot();
        if (repoRoot is not null)
        {
            string candidate = Combine(repoRoot, FixtureSegments);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        // Fallback: the copy that sits next to the benchmark binaries.
        string besideBinary = Combine(System.AppContext.BaseDirectory, FixtureSegments);
        if (File.Exists(besideBinary))
        {
            return besideBinary;
        }

        throw new FileNotFoundException(
            $"Could not locate the GBPUSD fixture. Looked for '{string.Join("/", FixtureSegments)}' " +
            $"relative to the repository root and to '{System.AppContext.BaseDirectory}'. " +
            $"Set {FixtureEnvironmentVariable} to point at the file.");
    }

    private static string? FindRepoRoot()
    {
        foreach (string start in new[] { System.AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        return null;
    }

    private static string Combine(string root, string[] segments)
    {
        string path = root;
        foreach (string segment in segments)
        {
            path = Path.Combine(path, segment);
        }

        return path;
    }
}
