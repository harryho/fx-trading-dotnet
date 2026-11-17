using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Xunit;
using Xunit.Abstractions;

namespace fxpa.ui;

/// <summary>
/// L5 - the UI smoke-test layer.
/// </summary>
/// <remarks>
/// <para>
/// The scenario is the real product path, minus the mouse:
/// </para>
/// <list type="number">
/// <item>the real <see cref="ChartControl"/> is created and shown on an STA
/// thread with a live WinForms message pump;</item>
/// <item>the bundled <c>GBPUSD</c> MIN1 fixture is loaded with the product's own
/// <c>fxpa.FileWorker.LoadCdlFile</c> (3,300 bars);</item>
/// <item>the bars are published through an <see cref="IDataProvider"/> and bound
/// to a real <see cref="TradeChartSeries"/> on the main pane;</item>
/// <item>a real indicator is taken from <see cref="FxpaIndicatorManager"/>,
/// initialised against the same provider, calculated and bound to the pane as
/// an <see cref="IndicatorChartSeries"/>;</item>
/// <item>the pane is rendered and the resulting bitmap is compared against the
/// bitmap of the very same pane without any series.</item>
/// </list>
/// <para>
/// No UIA pattern is used anywhere: invoking a WinForms handler through UIA
/// executes it on the UIA RPC thread, which deadlocks against the UI thread
/// (see the upgrade addenda SS9/SS10). The in-process approach sidesteps the
/// problem completely - no OS level input is involved at all.
/// </para>
/// </remarks>
public class UiSmokeTests
{
    /// <summary>Hard budget for the whole UI scenario - a hang must fail, not block.</summary>
    private static readonly TimeSpan ScenarioTimeout = TimeSpan.FromSeconds(60);

    private const string EmaDescription = "Exponential Moving Average";

    private readonly ITestOutputHelper _output;

    public UiSmokeTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Bundled_GBPUSD_fixture_is_pristine_and_yields_three_thousand_three_hundred_bars()
    {
        string path = BundledFixture.ResolveCdlPath();
        string sha = BundledFixture.ComputeSha256(path);

        Assert.Equal(BundledFixture.ExpectedSha256, sha);

        List<BarData> bars = BundledFixture.LoadCdl();

        Assert.Equal(BundledFixture.ExpectedBarCount, bars.Count);
        Assert.Equal(new DateTime(2010, 12, 27, 14, 40, 0), bars[0].DateTime);
        Assert.Equal(new DateTime(2010, 12, 31, 20, 59, 0), bars[^1].DateTime);
        Assert.Equal(1.5412, bars[0].Close, 6);
        Assert.Equal(1.5592, bars[^1].Close, 6);

        _output.WriteLine($"fixture   : {path}");
        _output.WriteLine($"sha256    : {sha}");
        _output.WriteLine($"bar count : {bars.Count}");
    }

    [Fact]
    public void An_interactive_desktop_session_is_available_for_the_ui_smoke_test()
    {
        // Creating a window is the only reliable probe for "there is a desktop
        // to draw on". On a headless agent (service account, session 0, CI
        // runner without an interactive session) this throws and the message
        // says so explicitly instead of hanging.
        using var host = new StaTestHost("fxpa.ui-probe-sta");

        bool handleCreated = host.Run(() =>
        {
            using var probe = new Form
            {
                Width = 120,
                Height = 120,
            };
            return probe.Handle != IntPtr.Zero;
        }, ScenarioTimeout);

        Assert.True(handleCreated,
            "A window handle could not be created - no interactive Windows desktop is available.");

        _output.WriteLine($"Environment.UserInteractive : {Environment.UserInteractive}");
        _output.WriteLine($"window handle created      : {handleCreated}");
    }

    [Fact]
    public void Chart_pane_renders_the_bundled_symbol_and_a_real_indicator()
    {
        SmokeReport report = RunChartScenario();

        _output.WriteLine($"bars                       : {report.BarCount}");
        _output.WriteLine($"pane series count          : {report.PaneSeriesCount}");
        _output.WriteLine($"price series values        : {report.PriceSeriesValueCount}");
        _output.WriteLine($"indicator                  : {report.IndicatorName} ({report.IndicatorDescription})");
        _output.WriteLine($"indicator values / finite  : {report.IndicatorSeriesValueCount} / {report.IndicatorFiniteValueCount}");
        _output.WriteLine($"indicator range            : {report.IndicatorMin} .. {report.IndicatorMax}");
        _output.WriteLine($"drawing space              : {report.DrawingSpaceWidth} x {report.DrawingSpaceHeight}");
        _output.WriteLine($"pixels empty->price        : {report.BaselineVsPricePixels}");
        _output.WriteLine($"pixels price->+indicator   : {report.PriceVsIndicatorPixels}");
        _output.WriteLine($"pixels empty->full         : {report.BaselineVsFullPixels}");
        _output.WriteLine($"non background pixels      : {report.NonBackgroundPixels}");
        _output.WriteLine($"dominant colour            : {report.DominantColor}");

        // --- the data made it into the chart -------------------------------
        Assert.Equal(BundledFixture.ExpectedBarCount, report.BarCount);
        Assert.Equal(2, report.PaneSeriesCount);

        // --- the indicator was really calculated ----------------------------
        Assert.Equal(BundledFixture.ExpectedBarCount, report.IndicatorSeriesValueCount);

        // TA-Lib needs a full period before the first EMA value: 14 periods of
        // warm-up, so 3,300 - 13 = 3,287 finite values are expected.
        Assert.Equal(BundledFixture.ExpectedBarCount - 13, report.IndicatorFiniteValueCount);

        // The EMA of the close price must live inside the price range.
        Assert.InRange(report.IndicatorMin, 1.50, 1.60);
        Assert.InRange(report.IndicatorMax, 1.50, 1.60);

        // --- the pane really drew the series --------------------------------
        // A blank pane and a populated pane cannot look the same.
        Assert.True(report.BaselineVsPricePixels > 500,
            $"The price series did not reach the screen: only {report.BaselineVsPricePixels} pixels differ " +
            "between the empty pane and the pane holding the candles.");

        // Adding the indicator must change the picture as well - this is the
        // assertion that the indicator series was painted, not merely stored.
        Assert.True(report.PriceVsIndicatorPixels > 20,
            $"The indicator series did not reach the screen: only {report.PriceVsIndicatorPixels} pixels differ " +
            "between the pane with candles and the pane with candles plus the indicator.");

        Assert.True(report.BaselineVsFullPixels > report.BaselineVsPricePixels,
            "Adding the indicator reduced the amount of drawn content - the render is not additive.");

        // The pane must not be blank, and it must not be a single flat colour.
        Assert.True(report.NonBackgroundPixels > 500,
            $"The rendered pane is blank: only {report.NonBackgroundPixels} non-background pixels.");

        // The drawing space must be a real, non-degenerate rectangle.
        Assert.True(report.DrawingSpaceWidth > 0f, "The chart drawing space has no width.");
        Assert.True(report.DrawingSpaceHeight > 0f, "The chart drawing space has no height.");
    }

    /// <summary>
    /// The whole scenario, executed on the STA thread.
    /// </summary>
    private static SmokeReport RunChartScenario()
    {
        using var host = new StaTestHost("fxpa.ui-smoke-sta");

        return host.Run<SmokeReport>(() =>
        {
            // ChartPane.UpdateDrawingSpace() dereferences the global
            // fxpa.AppContext.TradeAnalyzerControl; in the product it is set
            // while the platform is loaded. A bare instance is enough here: it
            // only answers "which provider serves this symbol?" and the answer
            // is "none", which is what the smoke test wants.
            fxpa.AppContext.TradeAnalyzerControl = new FxAnalyzerControl();

            using var form = new Form
            {
                Text = "FXPA UI smoke test",
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Location = new Point(-32000, -32000),
                ClientSize = new Size(1000, 700),
            };

            var chart = new ChartControl { Dock = DockStyle.Fill };
            form.Controls.Add(chart);

            // Realise the window: the pane only paints once it owns a handle.
            form.Show();
            Application.DoEvents();

            // --- 1. the empty pane -----------------------------------------
            using Bitmap baseline = PixelProbe.Capture(chart);

            // --- 2. the bundled symbol -------------------------------------
            List<BarData> bars = BundledFixture.LoadCdl();
            var provider = new FixtureDataProvider(bars);

            var priceSeries = new TradeChartSeries(BundledFixture.Symbol)
            {
                ChartType = TradeChartSeries.ChartTypeEnum.CandleStick,
            };
            priceSeries.Initialize(provider);

            // The product provider pushes the history through this event; the
            // chart series only pulls the bars out of the provider on it.
            provider.PublishInitialValues();

            chart.MainPane.Add(priceSeries, false, false);
            chart.MainPane.FitDrawingSpaceToScreen(true, true);
            Application.DoEvents();

            using Bitmap withPrice = PixelProbe.Capture(chart);

            // --- 3. a real indicator from the indicator manager -------------
            BasicIndicator indicator = CloneIndicatorByDescription(EmaDescription);

            // The product paints indicator result sets with the pens held by the
            // indicator UI; without a pen the series falls back to white.
            foreach (string resultSetName in indicator.UI.OutputResultSetsPens.Keys.ToArray())
            {
                indicator.UI.OutputResultSetsPens[resultSetName] = Pens.WhiteSmoke;
            }

            indicator.Initialize(provider);
            indicator.Calculate();

            var indicatorSeries = new IndicatorChartSeries(indicator.Name, indicator);
            chart.MainPane.Add(indicatorSeries);

            chart.MainPane.FitDrawingSpaceToScreen(true, true);
            Application.DoEvents();

            using Bitmap withIndicator = PixelProbe.Capture(chart);

            // --- 4. measurements --------------------------------------------
            float[] indicatorValues = ((SimpleChartSeries)indicatorSeries).ValueSets[0];
            double[] finite = indicatorValues
                .Where(v => !float.IsNaN(v) && !float.IsInfinity(v))
                .Select(v => (double)v)
                .ToArray();

            Color dominant = PixelProbe.DominantColor(withIndicator);

            // Opt-in debugging aid: FXPA_UI_SMOKE_DUMP=<dir> writes the three
            // renders to disk so a human can look at what the pane painted.
            string? dumpDirectory = Environment.GetEnvironmentVariable("FXPA_UI_SMOKE_DUMP");
            if (!string.IsNullOrEmpty(dumpDirectory))
            {
                Directory.CreateDirectory(dumpDirectory);
                baseline.Save(Path.Combine(dumpDirectory, "01-empty-pane.png"), ImageFormat.Png);
                withPrice.Save(Path.Combine(dumpDirectory, "02-candles.png"), ImageFormat.Png);
                withIndicator.Save(Path.Combine(dumpDirectory, "03-candles-plus-ema.png"), ImageFormat.Png);
            }

            return new SmokeReport
            {
                BarCount = bars.Count,
                PaneSeriesCount = chart.MainPane.SeriesCount,
                PriceSeriesValueCount = ((SimpleChartSeries)priceSeries).ValueSets[0].Length,
                IndicatorName = indicator.Name,
                IndicatorDescription = indicator.Description,
                IndicatorSeriesValueCount = indicatorValues.Length,
                IndicatorFiniteValueCount = finite.Length,
                IndicatorMin = finite.Length > 0 ? finite.Min() : double.NaN,
                IndicatorMax = finite.Length > 0 ? finite.Max() : double.NaN,
                DrawingSpaceWidth = chart.MainPane.DrawingSpaceDisplayLimit.Width,
                DrawingSpaceHeight = chart.MainPane.DrawingSpaceDisplayLimit.Height,
                BaselineVsPricePixels = PixelProbe.CountDifferingPixels(baseline, withPrice),
                PriceVsIndicatorPixels = PixelProbe.CountDifferingPixels(withPrice, withIndicator),
                BaselineVsFullPixels = PixelProbe.CountDifferingPixels(baseline, withIndicator),
                NonBackgroundPixels = PixelProbe.CountNonBackgroundPixels(withIndicator, dominant),
                DominantColor = dominant,
            };
        }, ScenarioTimeout);
    }

    /// <summary>
    /// Clones an indicator out of the product's indicator manager by its
    /// user facing description - the manager keys its catalogue by the TA-Lib
    /// method name ("Ema"), the description is what the user sees.
    /// </summary>
    private static BasicIndicator CloneIndicatorByDescription(string description)
    {
        FxpaIndicatorManager manager = FxpaIndicatorManager.Instance;

        string[] names = manager.GetIndicatorsNames(FxpaIndicatorManager.IndicatorGroups.Fxpa);
        string[] descriptions = manager.GetIndicatorsDescriptions(FxpaIndicatorManager.IndicatorGroups.Fxpa);

        for (int i = 0; i < names.Length && i < descriptions.Length; i++)
        {
            if (descriptions[i] == description)
            {
                return manager.GetIndicatorCloneByName(FxpaIndicatorManager.IndicatorGroups.Fxpa, names[i]);
            }
        }

        throw new InvalidOperationException(
            $"The indicator manager does not offer an indicator described as '{description}'.");
    }

    private sealed class SmokeReport
    {
        public int BarCount { get; init; }

        public int PaneSeriesCount { get; init; }

        public int PriceSeriesValueCount { get; init; }

        public string IndicatorName { get; init; } = string.Empty;

        public string IndicatorDescription { get; init; } = string.Empty;

        public int IndicatorSeriesValueCount { get; init; }

        public int IndicatorFiniteValueCount { get; init; }

        public double IndicatorMin { get; init; }

        public double IndicatorMax { get; init; }

        public float DrawingSpaceWidth { get; init; }

        public float DrawingSpaceHeight { get; init; }

        public int BaselineVsPricePixels { get; init; }

        public int PriceVsIndicatorPixels { get; init; }

        public int BaselineVsFullPixels { get; init; }

        public int NonBackgroundPixels { get; init; }

        public Color DominantColor { get; init; }
    }
}
