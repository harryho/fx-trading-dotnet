using System;
using System.Collections.Generic;

namespace fxpa.benchmarks;

/// <summary>
/// A minimal in-memory <see cref="IDataProvider"/> that lets the benchmark
/// drive the application's own indicator classes (<see cref="MFI"/>,
/// <see cref="EMA"/>) without a running data feed.
///
/// It mirrors <c>RemoteDataProvider.GetDataValues</c>: a fresh array is built
/// on every call, so the allocation cost that the real provider imposes is part
/// of the measured path rather than being optimised away by the harness.
/// </summary>
internal sealed class BenchmarkDataProvider : IDataProvider
{
    private readonly List<BarData> _units = new();

    internal BenchmarkDataProvider(double[] close, double[] high, double[] low, double[] volume, DateTime start)
    {
        for (int i = 0; i < close.Length; i++)
        {
            _units.Add(new BarData(start.AddMinutes(i), close[i], close[i], low[i], high[i], volume[i]));
        }
    }

    public string Name => "benchmark";

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

    /// <summary>
    /// The indicator subscribes to this event in <see cref="Indicator.Initialize"/>,
    /// but the benchmark never pushes an update, so the event stays inert.
    /// </summary>
    public event ValuesUpdatedDelegate ValuesUpdateEvent
    {
        add { }
        remove { }
    }

    public double[] GetDataValues(BarData.DataValueSourceEnum valueEnum)
        => GetDataValues(valueEnum, 0, _units.Count);

    public double[] GetDataValues(BarData.DataValueSourceEnum valueEnum, int startingIndex, int indexCount)
    {
        int count = indexCount == 0 ? _units.Count - startingIndex : indexCount;
        var result = new double[count];

        for (int i = 0; i < count; i++)
        {
            result[i] = _units[startingIndex + i].GetValue(valueEnum);
        }

        return result;
    }

    public int GetBarIndexAtTime(DateTime time) => 0;

    public void UnInitialize()
    {
    }
}
