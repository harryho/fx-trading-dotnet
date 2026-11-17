using System;

namespace fxpa.benchmarks;

/// <summary>
/// Deterministic market data used by every benchmark in this suite.
///
/// The series is generated from a fixed seed (<see cref="Seed"/>) so that two
/// runs - on two machines, before and after a refactor - feed the exact same
/// numbers into the code under measurement. Without that, a "regression" is
/// indistinguishable from a different sample.
/// </summary>
internal static class BenchmarkSeries
{
    /// <summary>Length of the price series the hot paths are measured against.</summary>
    internal const int DefaultLength = 5000;

    /// <summary>Fixed seed - change it only together with the recorded baselines.</summary>
    internal const int Seed = 42;

    /// <summary>
    /// A random walk around 1.1000 - close enough to a real GBPUSD quote that
    /// the indicator warm-up windows behave exactly as they do in production.
    /// </summary>
    internal static double[] CreateClosePrices(int length = DefaultLength)
    {
        var random = new Random(Seed);
        var close = new double[length];
        double price = 1.1000;

        for (int i = 0; i < length; i++)
        {
            price += (random.NextDouble() - 0.5) * 0.0020;
            close[i] = price;
        }

        return close;
    }

    /// <summary>
    /// Derives high/low envelopes around the close series. TA-Lib requires
    /// high &gt;= max(close) and low &lt;= min(close) for MFI to be meaningful.
    /// </summary>
    internal static void CreateHighLow(double[] close, out double[] high, out double[] low)
    {
        var random = new Random(Seed + 1);
        high = new double[close.Length];
        low = new double[close.Length];

        for (int i = 0; i < close.Length; i++)
        {
            double spread = 0.0002 + (random.NextDouble() * 0.0010);
            high[i] = close[i] + spread;
            low[i] = close[i] - spread;
        }
    }

    /// <summary>Positive tick volumes - TA-Lib's MFI rejects negative volume.</summary>
    internal static double[] CreateVolumes(int length = DefaultLength)
    {
        var random = new Random(Seed + 2);
        var volume = new double[length];

        for (int i = 0; i < length; i++)
        {
            volume[i] = 500.0 + (random.NextDouble() * 1500.0);
        }

        return volume;
    }

    /// <summary>
    /// A 5,000 point float series for the chart scanning benchmarks, in the
    /// magnitude range the chart code actually deals with.
    /// </summary>
    internal static float[] CreateFloatSeries(int length = DefaultLength)
    {
        var random = new Random(Seed);
        var values = new float[length];
        double value = 100.0;

        for (int i = 0; i < length; i++)
        {
            value += (random.NextDouble() - 0.5) * 2.0;
            values[i] = (float)value;
        }

        return values;
    }
}
