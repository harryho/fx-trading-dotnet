using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace fxpa.tests;

/// <summary>
/// Tests for the chart's coordinate mapping: how a point in pixel space
/// corresponds to a point in "drawing space" (where the X axis unit is
/// the bar index and the Y axis unit is the price), and how a time is
/// resolved to the bar that represents it.
///
/// Every mouse interaction and every painted candle goes through this
/// transform, so a silent change here shifts the whole chart.
/// </summary>
public class ChartCoordinateTests
{
    private static FakeDataProvider Provider()
    {
        var close = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var high = new double[] { 1.1, 2.1, 3.1, 4.1, 5.1 };
        var low = new double[] { 0.9, 1.9, 2.9, 3.9, 4.9 };
        var volume = new double[] { 10, 20, 30, 40, 50 };
        return new FakeDataProvider(close, high, low, volume, new DateTime(2010, 12, 27, 14, 40, 0));
    }

    // ---------------------------------------------------------------
    // time -> bar index
    // ---------------------------------------------------------------

    [Fact]
    public void Exact_time_maps_to_its_bar()
    {
        var provider = Provider();

        Assert.Equal(0, provider.GetBarIndexAtTime(new DateTime(2010, 12, 27, 14, 40, 0)));
        Assert.Equal(4, provider.GetBarIndexAtTime(new DateTime(2010, 12, 27, 14, 44, 0)));
    }

    [Fact]
    public void Time_between_bars_maps_to_the_next_bar()
    {
        var provider = Provider();

        // 14:41:30 falls between bar 1 (14:41) and bar 2 (14:42); the
        // contract is "first bar at or after the requested time".
        Assert.Equal(2, provider.GetBarIndexAtTime(new DateTime(2010, 12, 27, 14, 41, 30)));
    }

    [Fact]
    public void Time_outside_the_data_range_has_no_bar()
    {
        var provider = Provider();

        // Contract is "first bar at or after the requested time", so a
        // time before the data clamps to the first bar rather than
        // reporting "not found". Only a time past the last bar yields -1.
        Assert.Equal(0, provider.GetBarIndexAtTime(new DateTime(2000, 1, 1, 0, 0, 0)));
        Assert.Equal(-1, provider.GetBarIndexAtTime(new DateTime(2020, 1, 1, 0, 0, 0)));
    }

    // ---------------------------------------------------------------
    // pixel space <-> drawing space
    // ---------------------------------------------------------------

    [Fact]
    public void Default_transform_flips_the_y_axis()
    {
        var wrapper = new GraphicsWrapper();
        var elements = wrapper.DrawingSpaceTransform.Elements;

        // The constructor scales Y by -1: Win32 paints top to bottom,
        // while charts grow bottom up. X is untouched.
        Assert.Equal(1f, elements[0], 5);  // x scale
        Assert.Equal(-1f, elements[3], 5); // y scale - flipped!
        Assert.Equal(0f, elements[4], 5);  // x translate
        Assert.Equal(0f, elements[5], 5);  // y translate
    }

    [Fact]
    public void Pixel_and_drawing_space_round_trip_to_each_other()
    {
        var wrapper = new GraphicsWrapper();
        wrapper.ScaleDrawingSpaceTransform(7f, 0.4f);
        wrapper.TranslateDrawingSpaceTransfrom(123f, -55f);

        foreach (var point in new[]
                 {
                     new PointF(0f, 0f),
                     new PointF(1f, 100.5f),
                     new PointF(-30.25f, 7f),
                     new PointF(640f, 480f),
                 })
        {
            PointF drawing = wrapper.ActualSpaceToDrawingSpace(point, true);
            PointF actual = wrapper.DrawingSpaceToActualSpace(drawing, true);

            Assert.Equal(point.X, actual.X, 3);
            Assert.Equal(point.Y, actual.Y, 3);
        }
    }

    [Fact]
    public void One_drawing_unit_is_zoom_times_one_pixel()
    {
        var wrapper = new GraphicsWrapper();

        // At zoom 1, one bar spans one pixel.
        PointF zero = wrapper.DrawingSpaceToActualSpace(new PointF(0f, 0f), true);
        PointF oneBar = wrapper.DrawingSpaceToActualSpace(new PointF(1f, 0f), true);
        Assert.Equal(1f, oneBar.X - zero.X, 5);

        // Zoom in by 10: the same bar now spans ten pixels. Y is unaffected.
        wrapper.ScaleDrawingSpaceTransform(10f, 1f);
        zero = wrapper.DrawingSpaceToActualSpace(new PointF(0f, 0f), true);
        oneBar = wrapper.DrawingSpaceToActualSpace(new PointF(1f, 0f), true);
        Assert.Equal(10f, oneBar.X - zero.X, 4);
        Assert.Equal(0f, oneBar.Y - zero.Y, 5);
    }

    [Fact]
    public void Bar_index_n_paints_at_pixel_offset_n_times_bar_width()
    {
        // Default (Prepend) order: a pan applied before a zoom stays in
        // pixels - pixel = pan + zoom * bar. This is the composition the
        // chart's own zoom/pan handlers use.
        const float barWidth = 8f;
        var wrapper = new GraphicsWrapper();
        wrapper.TranslateDrawingSpaceTransfrom(100f, 0f); // pan so bar 0 starts at pixel 100
        wrapper.ScaleDrawingSpaceTransform(barWidth, 1f); // default order: Prepend

        PointF bar3 = wrapper.DrawingSpaceToActualSpace(new PointF(3f, 0f), true);
        Assert.Equal(124f, bar3.X, 4);
    }

    [Fact]
    public void Append_order_puts_the_scale_outside_the_pan()
    {
        // Contrast for the test above: with Append, the new scale is
        // applied after the accumulated transform, so the pan offset is
        // magnified by the zoom: pixel = zoom * (bar + pan). The codebase
        // never uses Append for zoom/pan; recorded to lock the semantics
        // of the two orders.
        const float barWidth = 8f;
        var wrapper = new GraphicsWrapper();
        wrapper.TranslateDrawingSpaceTransfrom(100f, 0f);
        wrapper.ScaleDrawingSpaceTransform(barWidth, 1f, MatrixOrder.Append);

        PointF bar3 = wrapper.DrawingSpaceToActualSpace(new PointF(3f, 0f), true);
        Assert.Equal(barWidth * (3f + 100f), bar3.X, 4); // 824
    }

    [Fact]
    public void Vector_mode_ignores_translation()
    {
        var wrapper = new GraphicsWrapper();
        wrapper.ScaleDrawingSpaceTransform(5f, 5f);
        wrapper.TranslateDrawingSpaceTransfrom(1000f, 2000f);

        // Vector mode transforms only direction and length - used for
        // measuring distances, not placing points. The Y component keeps
        // the constructor's downward flip: (1,1) maps to (5,-5).
        PointF vector = wrapper.DrawingSpaceToActualSpace(new PointF(1f, 1f), false);
        Assert.Equal(5f, vector.X, 5);
        Assert.Equal(-5f, vector.Y, 5);
    }

    [Fact]
    public void Non_invertible_transform_yields_origin_instead_of_throwing()
    {
        var wrapper = new GraphicsWrapper();
        wrapper.ScaleDrawingSpaceTransform(0f, 0f); // degenerate: not invertible
        Assert.False(wrapper.DrawingSpaceTransform.IsInvertible);

        PointF result = wrapper.ActualSpaceToDrawingSpace(new PointF(42f, 24f), true);
        Assert.Equal(0f, result.X, 5);
        Assert.Equal(0f, result.Y, 5);
    }

    [Fact]
    public void Sub_pane_synchronization_copies_x_but_not_y()
    {
        // The invariant behind pane alignment: every sub chart pane must
        // share the main pane's horizontal zoom and pan (so the candles
        // line up), while keeping its own vertical scale (each indicator
        // has its own price range).
        var master = new GraphicsWrapper();
        master.ScaleDrawingSpaceTransform(13f, 2f);
        master.TranslateDrawingSpaceTransfrom(40f, 500f);

        var slave = new GraphicsWrapper();
        slave.ScaleDrawingSpaceTransform(0.5f, 9f);
        slave.TranslateDrawingSpaceTransfrom(-17f, 3f);

        float[] slaveBefore = slave.DrawingSpaceTransform.Elements;
        slave.SynchronizeDrawingSpaceXAxis(master);
        var masterElements = master.DrawingSpaceTransform.Elements;
        var slaveElements = slave.DrawingSpaceTransform.Elements;

        // X follows the master (note: master's translate is carried in
        // already-zoomed matrix form, and the slave adopts it verbatim).
        Assert.Equal(masterElements[0], slaveElements[0], 5); // x scale copied
        Assert.Equal(masterElements[4], slaveElements[4], 5); // x translate copied

        // Y is untouched: whatever the sub pane's own vertical state was
        // (scale -9 from its y flip, translate -27 from composing its own
        // translate with that scale) survives the synchronization.
        Assert.Equal(slaveBefore[3], slaveElements[3], 5);
        Assert.Equal(slaveBefore[5], slaveElements[5], 5);
        Assert.Equal(-9f, slaveElements[3], 5);
    }
}
