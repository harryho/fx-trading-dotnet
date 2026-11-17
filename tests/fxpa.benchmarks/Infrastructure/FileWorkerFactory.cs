using System;

namespace fxpa.benchmarks;

/// <summary>
/// Creates the application's own file loader.
///
/// <c>fxpa.FileWorker</c> is an <c>internal</c> class and the fxpa assembly
/// only grants <c>InternalsVisibleTo("fxpa.tests")</c>, so this benchmark
/// assembly cannot name the type directly. Reaching it through reflection
/// keeps the measured code path identical to production
/// (<c>FileWorker.LoadCdlFile</c>) without touching anything under src/fxpa.
///
/// The instance is created once, outside the measured region, and is then
/// used through its public <see cref="IFileWorker"/> interface - so the
/// benchmarked call carries no reflection overhead at all.
/// </summary>
internal static class FileWorkerFactory
{
    private const string FileWorkerTypeName = "fxpa.FileWorker";

    internal static IFileWorker Create()
    {
        Type? type = typeof(BarData).Assembly.GetType(FileWorkerTypeName, throwOnError: true);
        if (type is null)
        {
            throw new InvalidOperationException($"Type '{FileWorkerTypeName}' was not found in the fxpa assembly.");
        }

        var instance = Activator.CreateInstance(type, nonPublic: true);
        if (instance is not IFileWorker worker)
        {
            throw new InvalidOperationException($"'{FileWorkerTypeName}' does not implement IFileWorker.");
        }

        return worker;
    }
}
