using System;
using BenchmarkDotNet.Attributes;
using TicTacTec.TA.Library;

namespace fxpa.benchmarks;

/// <summary>
/// Measures the raw TA-Lib RSI calculation - the single hottest numeric kernel
/// in the application. Every indicator the app draws is ultimately a TA-Lib
/// call, so this is the floor against which the FXPA wrappers are compared.
/// </summary>
[MemoryDiagnoser]
public class TaLibRsiBenchmarks
{
    private const int RsiPeriod = 14;

    private double[] _prices = Array.Empty<double>();
    private double[] _output = Array.Empty<double>();

    /// <summary>Number of prices fed to the indicator.</summary>
    [Params(5000)]
    public int SeriesLength { get; set; } = 5000;

    [GlobalSetup]
    public void Setup()
    {
        _prices = BenchmarkSeries.CreateClosePrices(SeriesLength);
        _output = new double[SeriesLength];
    }

    [Benchmark]
    public double Rsi()
    {
        Core.RetCode code = Core.Rsi(0, _prices.Length - 1, _prices, RsiPeriod,
            out int beginIndex, out int count, _output);

        if (code != Core.RetCode.Success)
        {
            throw new InvalidOperationException($"TA-Lib RSI failed with {code}.");
        }

        // Consume the last computed value so the call cannot be elided.
        return _output[count - 1];
    }
}
