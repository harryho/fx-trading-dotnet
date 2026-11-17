using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace fxpa.tests;

/// <summary>
/// Characterization tests for interval aggregation and the data
/// save/load pair.
///
/// The aggregation helpers decide which bar a tick belongs to; a change
/// there silently re-draws history at the wrong resolution. The save/load
/// pair decides what a user can export and re-import.
/// </summary>
public class IntervalAggregationTests
{
    /// <summary>
    /// The interval table is filled by an explicit startup call
    /// (AppConst.InitAppSettings) rather than by a static constructor, so
    /// a host that does not initialise the application - like this test
    /// host - sees an empty table and every aggregation call throws
    /// KeyNotFoundException. Initialize it once here.
    ///
    /// FINDING: InitAppSettings uses Dictionary.Add, so calling it twice
    /// throws. A static constructor or TryAdd would make it idempotent.
    /// </summary>
    private static void EnsureAppSettings()
    {
        if (!AppConst.AppTimeSpans.ContainsKey(Interval.MIN1))
        {
            AppConst.InitAppSettings();
        }
    }

    [Fact]
    public void Interval_durations_are_the_documented_ones()
    {
        EnsureAppSettings();

        Assert.Equal(TimeSpan.FromMinutes(1), AppConst.AppTimeSpans[Interval.MIN1]);
        Assert.Equal(TimeSpan.FromMinutes(3), AppConst.AppTimeSpans[Interval.MIN3]);
        Assert.Equal(TimeSpan.FromMinutes(5), AppConst.AppTimeSpans[Interval.MIN5]);
        Assert.Equal(TimeSpan.FromMinutes(15), AppConst.AppTimeSpans[Interval.MIN15]);
        Assert.Equal(TimeSpan.FromMinutes(30), AppConst.AppTimeSpans[Interval.MIN30]);
        Assert.Equal(TimeSpan.FromMinutes(60), AppConst.AppTimeSpans[Interval.MIN60]);
        Assert.Equal(TimeSpan.FromMinutes(120), AppConst.AppTimeSpans[Interval.MIN120]);
        Assert.Equal(TimeSpan.FromDays(1), AppConst.AppTimeSpans[Interval.DAY1]);
    }

    [Fact]
    public void Current_interval_time_advances_in_whole_intervals()
    {
        EnsureAppSettings();

        var last = new DateTime(2010, 12, 27, 14, 40, 0);

        // Inside the same 15-minute bucket: unchanged.
        Assert.Equal(last, AppUtil.GetCurrentIntervalTime(last, last.AddMinutes(14), Interval.MIN15));

        // Crossing the boundary: advances by exactly one interval.
        Assert.Equal(last.AddMinutes(15), AppUtil.GetCurrentIntervalTime(last, last.AddMinutes(15), Interval.MIN15));
        Assert.Equal(last.AddMinutes(30), AppUtil.GetCurrentIntervalTime(last, last.AddMinutes(30), Interval.MIN15));

        // A long gap advances by whole intervals only.
        Assert.Equal(last.AddMinutes(45), AppUtil.GetCurrentIntervalTime(last, last.AddMinutes(52), Interval.MIN15));
    }

    [Fact]
    public void Current_interval_time_is_stable_for_every_interval()
    {
        EnsureAppSettings();

        var last = new DateTime(2010, 12, 27, 14, 40, 0);
        foreach (Interval interval in Enum.GetValues<Interval>())
        {
            TimeSpan span = AppConst.AppTimeSpans[interval];
            DateTime stepped = AppUtil.GetCurrentIntervalTime(last, last.Add(span), interval);
            Assert.Equal(last.Add(span), stepped);
        }
    }

    [Fact]
    public void Latest_interval_time_keeps_the_previous_bucket_for_a_dense_series()
    {
        EnsureAppSettings();

        var last = new DateTime(2010, 12, 27, 14, 40, 0);

        // One full interval later: the bucket is still the old one.
        Assert.Equal(last, AppUtil.GetLatestIntervalTime(last, last.AddMinutes(15), Interval.MIN15));
    }

    [Fact]
    public void Latest_interval_time_does_not_advance_across_a_two_interval_gap()
    {
        // FINDING (characterized, not fixed): GetLatestIntervalTime only
        // advances when the gap is SMALLER than one interval (it returns
        // the new time); for a gap of one interval or more it returns the
        // previous bucket start unchanged.
        //
        // The only caller (DataCache.UpdateIntervalData) invokes it exactly
        // in that "gap >= one interval" case, to stamp the bar that just
        // closed. For a dense series that is correct - the closed bar did
        // start at the previous bucket time. For a gap of two intervals or
        // more it looks one interval stale: the closed bar is re-stamped
        // with the bucket before it.
        //
        // Affects the live/streaming rollover path (the bundled file data
        // is loaded directly and never goes through this code). Left as
        // is: changing it would shift bar timestamps on screen, which is
        // a product decision, not a refactor.
        EnsureAppSettings();

        var last = new DateTime(2010, 12, 27, 14, 40, 0);

        // Gap smaller than the interval: advances to the new time.
        Assert.Equal(last.AddMinutes(5),
            AppUtil.GetLatestIntervalTime(last, last.AddMinutes(5), Interval.MIN15));

        // Gap of exactly one interval: unchanged.
        Assert.Equal(last,
            AppUtil.GetLatestIntervalTime(last, last.AddMinutes(15), Interval.MIN15));

        // Gap of two intervals: still unchanged (the suspected off-by-one).
        Assert.Equal(last,
            AppUtil.GetLatestIntervalTime(last, last.AddMinutes(30), Interval.MIN15));

        // Gap of ten intervals: still unchanged.
        Assert.Equal(last,
            AppUtil.GetLatestIntervalTime(last, last.AddMinutes(150), Interval.MIN15));
    }

    [Fact]
    public void Cdl_save_and_load_are_not_round_trip_compatible()
    {
        // FINDING (characterized, not fixed): FileWorker.SaveCdlFile writes
        // "yyyy-MM-dd HH:mm,open,close,low,high" while LoadCdlFile expects
        // the bundled format "yyyyMMdd,HHmmss,open,high,low,close". The
        // two are not compatible, so a file written by the application
        // cannot be read back by its own loader - every row is skipped.
        //
        // This test pins the current behaviour so the mismatch cannot
        // change silently. If it is ever fixed, update this test and the
        // note in the README.
        var path = Path.Combine(Path.GetTempPath(), $"fxpa-tests-{Guid.NewGuid():N}.cdl");
        try
        {
            var bars = new[]
            {
                new BarData(new DateTime(2010, 12, 27, 14, 40, 0), 99.0, 99.5, 97.0, 99.6, 0),
                new BarData(new DateTime(2010, 12, 27, 14, 41, 0), 99.5, 100.0, 97.8, 100.1, 0),
            };

            var worker = new FileWorker();
            Assert.True(worker.SaveCdlFile(path, bars, false), "SaveCdlFile failed");

            string written = File.ReadAllText(path);
            Assert.Contains("2010-12-27 14:40", written);
            Assert.DoesNotContain("20101227,144000", written);

            bool ok = worker.LoadCdlFile(path, 0, int.MaxValue, out var loaded, true, true);
            Assert.True(ok, "LoadCdlFile reported failure");
            Assert.Empty(loaded); // every row is skipped: the formats differ
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Csv_round_trip_preserves_ohlc_and_volume()
    {
        // BarDataHelper's CSV writer/reader pair is self-consistent, and
        // this pins the exact column order it uses.
        var path = Path.Combine(Path.GetTempPath(), $"fxpa-tests-{Guid.NewGuid():N}.csv");
        try
        {
            var original = new[]
            {
                new BarData(new DateTime(2010, 12, 27, 14, 40, 0), 99.0, 99.5, 97.0, 99.6, 1234),
                new BarData(new DateTime(2010, 12, 27, 14, 41, 0), 99.5, 100.0, 97.8, 100.1, 5678),
            };

            File.WriteAllLines(path, original.Select(b =>
                $"{b.DateTime:dd.MM.yyyy},{b.DateTime:HH:mm},{b.Open},{b.High},{b.Low},{b.Close},{b.Volume}"));

            Assert.True(BarDataHelper.LoadFromFileCSV(path, 0, 0, out var loaded));

            Assert.Equal(original.Length, loaded.Count);
            for (int i = 0; i < original.Length; i++)
            {
                Assert.Equal(original[i].DateTime, loaded[i].DateTime);
                Assert.Equal(original[i].Open, loaded[i].Open, 6);
                Assert.Equal(original[i].High, loaded[i].High, 6);
                Assert.Equal(original[i].Low, loaded[i].Low, 6);
                Assert.Equal(original[i].Close, loaded[i].Close, 6);
                Assert.Equal(original[i].Volume, loaded[i].Volume, 6);
            }
        }
        finally
        {
            File.Delete(path);
        }
    }
}
