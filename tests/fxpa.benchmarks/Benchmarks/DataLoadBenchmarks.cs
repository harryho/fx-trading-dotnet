using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace fxpa.benchmarks;

/// <summary>
/// Measures loading the bundled GBPUSD MIN1 history file
/// (<c>src/fxpa/fxd/GBPUSD/MIN1/local/GBPUSD.cdl</c>, 5,320 lines) through the
/// application's own <c>FileWorker.LoadCdlFile</c>.
///
/// The loader is expected to keep 3,300 of the 5,320 rows: 2,020 rows carry a
/// minute field without leading zeros (for example "900" instead of "090000"),
/// which the loader cannot parse and drops silently. That quirk is locked by
/// the unit tests and is part of the behaviour being baselined here.
/// </summary>
[MemoryDiagnoser]
public class DataLoadBenchmarks
{
    private IFileWorker _worker = null!;
    private string _fixturePath = null!;

    [GlobalSetup]
    public void Setup()
    {
        _fixturePath = RepoPaths.FixturePath;
        _worker = FileWorkerFactory.Create();
    }

    [Benchmark]
    public int LoadCdlFile()
    {
        bool ok = _worker.LoadCdlFile(_fixturePath, 0, int.MaxValue, out List<BarData> bars, true, true);
        if (!ok)
        {
            throw new InvalidOperationException("LoadCdlFile reported failure.");
        }

        return bars.Count;
    }
}
