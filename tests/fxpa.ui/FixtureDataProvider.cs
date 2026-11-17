using System;
using System.Collections.Generic;

namespace fxpa.ui;

/// <summary>
/// A deterministic, in-memory <see cref="IDataProvider"/> over the bundled
/// GBPUSD bars.
/// </summary>
/// <remarks>
/// The product's own provider (<see cref="DataProvider"/>) is a network client:
/// it starts timers, registers with <c>ProviderService</c> and downloads the
/// history file. None of that is deterministic, and any of it can block on a
/// socket - exactly the kind of thing a smoke test must not do. This fake
/// serves the very same bars that
/// <c>fxpa.FileWorker.LoadCdlFile</c> produced, so the chart, the indicator
/// manager and the TA-Lib wrappers all see production-shaped data.
/// </remarks>
internal sealed class FixtureDataProvider : IDataProvider
{
    private readonly List<BarData> _units;

    public FixtureDataProvider(IEnumerable<BarData> bars)
    {
        _units = new List<BarData>(bars);
    }

    public string Name => "fxpa.ui-smoke-fixture";

    public double Ask => _units.Count > 0 ? _units[^1].Close : double.NaN;

    public double Bid => _units.Count > 0 ? _units[^1].Close : double.NaN;

    public double Spread => 0d;

    public int DataUnitCount => _units.Count;

    public ITimeControl? TimeControl => null;

    public DateTime FirstBarDateTime => _units.Count > 0 ? _units[0].DateTime : DateTime.MinValue;

    public DateTime LastBarDateTime => _units.Count > 0 ? _units[^1].DateTime : DateTime.MinValue;

    public IList<BarData> DataUnits => _units;

    public DateTime CurrentSourceDateTime => LastBarDateTime;

    public BarData? CurrentDataUnit => _units.Count > 0 ? _units[^1] : null;

    public TimeSpan TimeInterval => TimeSpan.FromMinutes(1);

    public event ValuesUpdatedDelegate? ValuesUpdateEvent;

    public double[] GetDataValues(BarData.DataValueSourceEnum valueEnum)
        => GetDataValues(valueEnum, 0, _units.Count);

    public double[] GetDataValues(BarData.DataValueSourceEnum valueEnum, int startingIndex, int indexCount)
    {
        if (startingIndex < 0 || startingIndex > _units.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(startingIndex));
        }

        int count = Math.Min(indexCount, _units.Count - startingIndex);
        if (count < 0)
        {
            count = 0;
        }

        var result = new double[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = _units[startingIndex + i].GetValue(valueEnum);
        }

        return result;
    }

    public int GetBarIndexAtTime(DateTime time)
    {
        for (int i = 0; i < _units.Count; i++)
        {
            if (_units[i].DateTime >= time)
            {
                return i;
            }
        }

        return -1;
    }

    public void UnInitialize()
    {
        ValuesUpdateEvent = null;
    }

    /// <summary>
    /// Publishes the initial snapshot, exactly like the product provider does
    /// when a session starts. <see cref="TradeChartSeries"/> only pulls the bars
    /// out of the provider in reaction to this event.
    /// </summary>
    public void PublishInitialValues()
    {
        ValuesUpdateEvent?.Invoke(this, UpdateType.Initial, _units.Count, 0);
    }
}
