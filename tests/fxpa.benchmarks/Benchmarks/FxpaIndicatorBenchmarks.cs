using System;
using BenchmarkDotNet.Attributes;

namespace fxpa.benchmarks;

/// <summary>
/// Measures the FXPA indicator wrappers end to end: the data provider
/// extraction, the TA-Lib call, the result-set bookkeeping and the
/// crossing/extremum signal analysis that runs afterwards.
///
/// This is the number that matters for the user visible latency of the
/// analyzer pane - it is deliberately much more expensive than the bare
/// TA-Lib kernel measured by <see cref="TaLibRsiBenchmarks"/>.
/// </summary>
[MemoryDiagnoser]
public class FxpaIndicatorBenchmarks
{
    private BenchmarkDataProvider _provider = null!;
    private MFI _mfi = null!;
    private EMA _ema = null!;

    [GlobalSetup]
    public void Setup()
    {
        double[] close = BenchmarkSeries.CreateClosePrices();
        double[] high;
        double[] low;
        BenchmarkSeries.CreateHighLow(close, out high, out low);
        double[] volume = BenchmarkSeries.CreateVolumes(close.Length);

        _provider = new BenchmarkDataProvider(
            close, high, low, volume,
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        _mfi = new MFI();
        _mfi.Initialize(_provider);

        _ema = new EMA();
        _ema.Initialize(_provider);
    }

    /// <summary>Money Flow Index - four data extractions plus the TA-Lib MFI kernel.</summary>
    [Benchmark(Baseline = true)]
    public void MoneyFlowIndex()
    {
        _mfi.Calculate();
    }

    /// <summary>Exponential Moving Average - one data extraction plus the TA-Lib EMA kernel.</summary>
    [Benchmark]
    public void ExponentialMovingAverage()
    {
        _ema.Calculate();
    }
}
