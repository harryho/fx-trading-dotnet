using System;
using System.Collections.Generic;
using System.Linq;

namespace fxpa.tests;

/// <summary>
/// A minimal in-memory <see cref="IDataProvider"/> used to drive the
/// application's own indicator classes from tests. It serves the same
/// deterministic series the golden fixtures were generated from, so the
/// FXPA wrappers can be compared against the canonical TA-Lib values.
/// </summary>
public sealed class FakeDataProvider : IDataProvider
{
    private readonly List<BarData> _units = new();

    public FakeDataProvider(double[] close, double[] high, double[] low, double[] volume, DateTime start)
    {
        for (int i = 0; i < close.Length; i++)
        {
            _units.Add(new BarData(start.AddMinutes(i), close[i], close[i], low[i], high[i], volume[i]));
        }
    }

    /// <summary>
    /// Builds a provider from explicit OHLC bars, for indicators that need
    /// real candle geometry (pin bars, shadows).
    /// </summary>
    public FakeDataProvider(IEnumerable<BarData> bars)
    {
        _units.AddRange(bars);
    }

    public string Name => "fake";

    public double Ask => 0;

    public double Bid => 0;

    public double Spread => 0;

    public int DataUnitCount => _units.Count;

    public ITimeControl TimeControl => null!;

    public DateTime FirstBarDateTime => _units[0].DateTime;

    public DateTime LastBarDateTime => _units[^1].DateTime;

    public IList<BarData> DataUnits => _units;

    public DateTime CurrentSourceDateTime => LastBarDateTime;

    public BarData? CurrentDataUnit => _units[^1];

    public TimeSpan TimeInterval => TimeSpan.FromMinutes(1);

    public event ValuesUpdatedDelegate ValuesUpdateEvent
    {
        add { }
        remove { }
    }

    public double[] GetDataValues(BarData.DataValueSourceEnum valueEnum)
        => GetDataValues(valueEnum, 0, _units.Count);

    public double[] GetDataValues(BarData.DataValueSourceEnum valueEnum, int startingIndex, int indexCount)
    {
        int end = Math.Min(startingIndex + indexCount, _units.Count);
        var slice = _units.Skip(startingIndex).Take(Math.Max(0, end - startingIndex)).ToList();

        return valueEnum switch
        {
            BarData.DataValueSourceEnum.Open => slice.Select(b => b.Open).ToArray(),
            BarData.DataValueSourceEnum.High => slice.Select(b => b.High).ToArray(),
            BarData.DataValueSourceEnum.Low => slice.Select(b => b.Low).ToArray(),
            BarData.DataValueSourceEnum.Close => slice.Select(b => b.Close).ToArray(),
            BarData.DataValueSourceEnum.Volume => slice.Select(b => b.Volume).ToArray(),
            // BarData.Average is (Open + Low + High + Close) / 4.
            BarData.DataValueSourceEnum.Average =>
                slice.Select(b => (b.Open + b.Low + b.High + b.Close) / 4d).ToArray(),
            _ => throw new NotSupportedException($"unsupported value source: {valueEnum}"),
        };
    }

    public int GetBarIndexAtTime(DateTime time)
        => _units.FindIndex(b => b.DateTime >= time);

    public void UnInitialize()
    {
    }
}
