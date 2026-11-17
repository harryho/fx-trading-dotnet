using System;
using BenchmarkDotNet.Attributes;

namespace fxpa.benchmarks;

/// <summary>
/// Measures the minimum/maximum scan the chart performs on every repaint to
/// work out the visible price range. <see cref="SimpleChartSeries"/> is the
/// plain (non-candle) series implementation, and both scans are pure public
/// API - no window, no handle, no message loop required.
/// </summary>
[MemoryDiagnoser]
public class ChartSeriesScanBenchmarks
{
    private SimpleChartSeries _series = null!;
    private float[] _values = Array.Empty<float>();

    [GlobalSetup]
    public void Setup()
    {
        _values = BenchmarkSeries.CreateFloatSeries();
        _series = new SimpleChartSeries(
            "benchmark",
            SimpleChartSeries.ChartTypeEnum.Line,
            _values);
    }

    /// <summary>Lowest value of the series - the lower bound of the visible range.</summary>
    [Benchmark(Baseline = true)]
    public float TotalMinimum()
    {
        return _series.GetTotalMinimum(0, _values.Length);
    }

    /// <summary>Highest value of the series - the upper bound of the visible range.</summary>
    [Benchmark]
    public float TotalMaximum()
    {
        return _series.GetTotalMaximum(0, _values.Length);
    }
}
