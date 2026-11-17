using System;
using System.IO;
using System.Security.Cryptography;
using Xunit;

namespace fxpa.tests;

/// <summary>
/// Characterization tests for the offline data pipeline over the bundled
/// GBPUSD fixture. They lock CURRENT behaviour — including the loader's
/// habit of silently skipping malformed rows — so a refactor of the data
/// layer cannot change what reaches the chart unnoticed.
///
/// Fixture facts (verified independently):
///   - 5,320 rows, 2010-12-27 14:40 .. 2010-12-31 20:59 (MIN1 bars).
///   - 2,020 rows carry times without leading zeros (e.g. "900" instead
///     of "090000"); FileWorker.LoadCdlFile cannot parse those and drops
///     them silently, leaving 3,300 bars. Known quirk — deliberately
///     locked here, not silently "fixed".
/// </summary>
public class DataPipelineTests
{
    private const string FixtureSha256 =
        "73b6a63262d2b31a3fd7901d89fddfb476ca21596078594ecb45510eed685201";
    private const int FixtureLineCount = 5320;
    private const int LoadedBarCount = 3300;

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(System.AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "fx-trading-dotnet.sln")))
        {
            dir = dir.Parent!;
        }

        Assert.NotNull(dir);
        return dir!.FullName;
    }

    private static string CdlPath()
        => Path.Combine(RepoRoot(), "src", "fxpa", "fxd", "GBPUSD", "MIN1", "local", "GBPUSD.cdl");

    private static System.Collections.Generic.List<BarData> LoadCdl()
    {
        var worker = new FileWorker();
        bool ok = worker.LoadCdlFile(CdlPath(), 0, int.MaxValue, out var bars, true, true);
        Assert.True(ok, "LoadCdlFile reported failure");
        return bars;
    }

    [Fact]
    public void Bundled_fixture_is_unmodified()
    {
        byte[] bytes = File.ReadAllBytes(CdlPath());
        string sha = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        Assert.Equal(FixtureSha256, sha);
    }

    [Fact]
    public void Loader_reads_exactly_the_valid_rows()
    {
        var bars = LoadCdl();
        int totalLines = File.ReadAllLines(CdlPath()).Length;

        Assert.Equal(FixtureLineCount, totalLines);
        Assert.Equal(LoadedBarCount, bars.Count);
    }

    [Fact]
    public void First_and_last_bars_are_locked()
    {
        var bars = LoadCdl();

        var first = bars[0];
        Assert.Equal(new DateTime(2010, 12, 27, 14, 40, 0), first.DateTime);
        Assert.Equal(1.5412, first.Open, 6);
        Assert.Equal(1.5412, first.High, 6);
        Assert.Equal(1.5411, first.Low, 6);
        Assert.Equal(1.5412, first.Close, 6);

        var last = bars[^1];
        Assert.Equal(new DateTime(2010, 12, 31, 20, 59, 0), last.DateTime);
        Assert.Equal(1.5591, last.Open, 6);
        Assert.Equal(1.5592, last.High, 6);
        Assert.Equal(1.5591, last.Low, 6);
        Assert.Equal(1.5592, last.Close, 6);
    }

    [Fact]
    public void Bars_are_strictly_time_ascending()
    {
        var bars = LoadCdl();
        for (int i = 1; i < bars.Count; i++)
        {
            Assert.True(bars[i].DateTime > bars[i - 1].DateTime,
                $"bar {i} ({bars[i].DateTime}) not after previous ({bars[i - 1].DateTime})");
        }
    }

    [Fact]
    public void Bars_are_high_low_consistent_and_completed()
    {
        var bars = LoadCdl();
        foreach (var bar in bars)
        {
            Assert.True(bar.High >= bar.Low, $"high<low at {bar.DateTime}");
            Assert.True(bar.IsCompleted, $"bar {bar.DateTime} not completed");
        }
    }

    [Fact]
    public void Cdl_source_carries_no_volume()
    {
        var bars = LoadCdl();
        // cdl rows carry no volume; the loader leaves the struct default
        // (0.0) in place. A non-zero value would mean the parse path changed.
        Assert.Equal(0.0, bars[0].Volume, 6);
    }

    [Fact]
    public void Csv_loader_column_order_is_open_high_low_close()
    {
        string path = Path.Combine(Path.GetTempPath(), $"fxpa-tests-{Guid.NewGuid():N}.csv");
        try
        {
            File.WriteAllText(path, "27.12.2010,14:40,1.1,1.2,1.3,1.4,1500\r\n");

            bool ok = BarDataHelper.LoadFromFileCSV(path, 0, 0, out var bars);
            Assert.True(ok, "LoadFromFileCSV reported failure");
            Assert.Single(bars);

            var bar = bars[0];
            Assert.Equal(new DateTime(2010, 12, 27, 14, 40, 0), bar.DateTime);
            Assert.Equal(1.1, bar.Open, 6);   // column 3
            Assert.Equal(1.2, bar.High, 6);   // column 4
            Assert.Equal(1.3, bar.Low, 6);    // column 5
            Assert.Equal(1.4, bar.Close, 6);  // column 6
            Assert.Equal(1500, bar.Volume, 6); // column 7
        }
        finally
        {
            File.Delete(path);
        }
    }
}
