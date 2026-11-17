using System;
using Xunit;

namespace fxpa.tests;

/// <summary>
/// Locks the chart data-scaling math (ChartPane.GetSingleSeriesMinMax):
/// how a pane's price window derives from series extremes — including the
/// 10% head-room rule and its 0.001 floor ("Harry -- Modified" padding).
/// These values are what positions every pixel of every price axis, so a
/// regression here shows up as "the chart looks wrong" unless pinned.
/// </summary>
public class ChartScaleTests
{
    private sealed class TestablePane : ChartPane
    {
        public (float Min, float Max) MinMaxFor(ChartSeries series, int? startIndex, int? endIndex)
        {
            float min = float.MaxValue;
            float max = float.MinValue;
            GetSingleSeriesMinMax(series, startIndex, endIndex, ref min, ref max);
            return (min, max);
        }
    }

    private static SimpleChartSeries Series(params float[] values)
        => new("test", SimpleChartSeries.ChartTypeEnum.Line, values);

    [Fact]
    public void Adds_ten_percent_headroom_around_the_series_extremes()
    {
        using var pane = new TestablePane();
        var (min, max) = pane.MinMaxFor(Series(10f, 11f, 12f, 13f, 14f), null, null);

        // difference = (14-10)/20 = 0.2; padding = 2 * 0.2
        Assert.Equal(9.6f, min, 3f);
        Assert.Equal(14.4f, max, 3f);
    }

    [Fact]
    public void Uses_the_full_range_ignoring_index_defaults()
    {
        using var pane = new TestablePane();
        var (min, max) = pane.MinMaxFor(Series(10f, 20f), null, null);

        Assert.Equal(9.0f, min, 3f);
        Assert.Equal(21.0f, max, 3f);
    }

    [Fact]
    public void Flat_series_get_the_minimum_headroom_floor()
    {
        using var pane = new TestablePane();
        var (min, max) = pane.MinMaxFor(Series(5f, 5f, 5f), null, null);

        // difference floor = 0.001001; padding = 2 * floor
        Assert.Equal(5f - 0.002002f, min, 6f);
        Assert.Equal(5f + 0.002002f, max, 6f);
    }

    [Fact]
    public void Nan_values_are_skipped()
    {
        using var pane = new TestablePane();
        var (min, max) = pane.MinMaxFor(Series(10f, float.NaN, 14f), null, null);

        Assert.Equal(9.6f, min, 3f);
        Assert.Equal(14.4f, max, 3f);
    }

    [Fact]
    public void Explicit_range_limits_the_window()
    {
        using var pane = new TestablePane();
        // Window [2, 4) over {10, 11, 12, 13, 14} -> values {12, 13}
        var (min, max) = pane.MinMaxFor(Series(10f, 11f, 12f, 13f, 14f), 2, 4);

        Assert.Equal(11.9f, min, 3f);
        Assert.Equal(13.1f, max, 3f);
    }
}
