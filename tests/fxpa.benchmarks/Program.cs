using System;
using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace fxpa.benchmarks;

/// <summary>
/// Entry point of the FXPA performance baseline suite.
///
/// The suite is run on demand - never as part of <c>dotnet test</c> - so that
/// a slow or noisy machine cannot turn the unit-test run red.
///
/// Usage examples:
///   dotnet run -c Release --project tests/fxpa.benchmarks -- --job short
///   dotnet run -c Release --project tests/fxpa.benchmarks -- --filter *Rsi* --job short
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        // Resolve the bundled fixture here, in the parent process, and hand
        // the absolute path to the benchmark child processes through the
        // environment - their working directory is not the repository.
        RepoPaths.PublishFixturePathToChildProcesses();

        // BenchmarkSwitcher.Run returns one summary per benchmark class; the
        // process exit code reports whether every benchmark succeeded.
        var summaries = BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly())
            .Run(args, BuildConfig());

        int failed = 0;
        foreach (var summary in summaries)
        {
            if (summary.HasCriticalValidationErrors)
            {
                failed++;
            }
        }

        return failed == 0 ? 0 : 1;
    }

    /// <summary>
    /// The default BenchmarkDotNet configuration with the optimizations
    /// validator switched off.
    ///
    /// The validator aborts the run when a referenced dependency was not
    /// compiled in Release. <c>src/lib/TA-Lib-Core.dll</c> is a pre-built
    /// binary that ships with the repository - there is no source to rebuild -
    /// so that check can never pass here. Every other reference (fxpa.dll,
    /// this assembly) is built in Release.
    /// </summary>
    private static IConfig BuildConfig()
    {
        ManualConfig config = ManualConfig.Create(DefaultConfig.Instance);
        config.WithOptions(ConfigOptions.DisableOptimizationsValidator);
        return config;
    }
}
