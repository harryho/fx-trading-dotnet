using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace fxpa.ui;

/// <summary>
/// Access to the bundled GBPUSD MIN1 fixture.
/// </summary>
/// <remarks>
/// The fixture is read with the application's own loader,
/// <c>fxpa.FileWorker.LoadCdlFile</c>, so the smoke test exercises exactly the
/// parsing path the product uses.
///
/// <c>fxpa.FileWorker</c> is an <c>internal</c> class and the only
/// <c>InternalsVisibleTo</c> grant in the product assembly is for
/// <c>fxpa.tests</c>. This project is not allowed to edit
/// <c>src/fxpa/Properties/AssemblyInfo.cs</c>, so the loader is reached through
/// reflection. If the type is ever renamed or made public, the reflection
/// fallback below stops being used and the code keeps working unchanged.
/// </remarks>
internal static class BundledFixture
{
    public const string Symbol = "GBPUSD";
    public const string TimeFrame = "MIN1";

    /// <summary>SHA-256 of the pristine fixture (also asserted by fxpa.tests).</summary>
    public const string ExpectedSha256 =
        "73b6a63262d2b31a3fd7901d89fddfb476ca21596078594ecb45510eed685201";

    /// <summary>Number of rows the loader keeps (5,320 rows, 2,020 unparsable).</summary>
    public const int ExpectedBarCount = 3300;

    /// <summary>
    /// Absolute path of the bundled fixture inside the repository.
    /// </summary>
    public static string RepositoryCdlPath()
        => Path.Combine(
            RepositoryRoot(),
            "src", "fxpa", "fxd", Symbol, TimeFrame, "local", Symbol + ".cdl");

    /// <summary>
    /// The fixture as copied next to the test binaries (the product project
    /// marks src/fxpa/fxd/** as CopyToOutputDirectory content, so the file
    /// travels with the build output as well).
    /// </summary>
    public static string LocalCdlPath()
        => Path.Combine(
            System.AppContext.BaseDirectory,
            "fxd", Symbol, TimeFrame, "local", Symbol + ".cdl");

    /// <summary>
    /// Resolves the fixture: the copy next to the test binary first (works
    /// from a published output), the repository copy as a fallback.
    /// </summary>
    public static string ResolveCdlPath()
    {
        string local = LocalCdlPath();
        if (File.Exists(local))
        {
            return local;
        }

        string repository = RepositoryCdlPath();
        if (File.Exists(repository))
        {
            return repository;
        }

        throw new FileNotFoundException(
            $"The bundled {Symbol} fixture was not found. Looked at '{local}' and '{repository}'.",
            local);
    }

    /// <summary>Walks up from the test binaries to the repository root.</summary>
    public static string RepositoryRoot()
    {
        var dir = new DirectoryInfo(System.AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "fx-trading-dotnet.sln")))
        {
            dir = dir.Parent;
        }

        if (dir is null)
        {
            throw new DirectoryNotFoundException(
                "Could not locate the repository root (fx-trading-dotnet.sln) from '" +
                System.AppContext.BaseDirectory + "'.");
        }

        return dir.FullName;
    }

    /// <summary>
    /// Loads the fixture with the product's own <c>fxpa.FileWorker</c>.
    /// </summary>
    public static List<BarData> LoadCdl()
    {
        string path = ResolveCdlPath();

        Assembly productAssembly = typeof(ChartControl).Assembly;
        Type? workerType = productAssembly.GetType("fxpa.FileWorker", throwOnError: false);

        if (workerType is null)
        {
            throw new InvalidOperationException(
                "fxpa.FileWorker was not found in the product assembly. The fixture loader " +
                "contract changed - update BundledFixture.LoadCdlFile.");
        }

        object worker = Activator.CreateInstance(workerType, nonPublic: true)
            ?? throw new InvalidOperationException("Could not instantiate fxpa.FileWorker.");

        MethodInfo loadCdlFile = workerType.GetMethod(
            "LoadCdlFile",
            BindingFlags.Instance | BindingFlags.Public)
            ?? throw new InvalidOperationException("fxpa.FileWorker.LoadCdlFile was not found.");

        object?[] arguments =
        {
            path,
            0,              // startingRow
            int.MaxValue,   // rowsLimit
            null,           // out List<BarData> datas
            true,           // isDecrypt
            true,           // isLocal
        };

        bool ok = (bool)loadCdlFile.Invoke(worker, arguments)!;
        var bars = (List<BarData>)arguments[3]!;

        if (!ok)
        {
            throw new InvalidOperationException(
                $"fxpa.FileWorker.LoadCdlFile('{path}') reported failure.");
        }

        return bars;
    }

    /// <summary>SHA-256 of the fixture file, used to prove the bundled data is pristine.</summary>
    public static string ComputeSha256(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
