# UI smoke tests (L5)

A tiny suite that proves the chart actually renders: real `ChartControl`,
real data fixture, real indicator, real pixels. It is deliberately **not**
wired into `fx-trading-dotnet.sln` - it needs a Windows desktop session and
must never be able to hang the ordinary unit-test run.

```
dotnet test tests/fxpa.ui/Fxpa.UiSmoke.csproj -c Release
```

## What the main scenario does

1. Creates the real `ChartControl` on an STA thread with a live WinForms
   message pump.
2. Loads the bundled `GBPUSD` MIN1 fixture with the product's own
   `FileWorker.LoadCdlFile` (3,300 bars, SHA-256 pinned).
3. Publishes the bars through an `IDataProvider` and binds them to a real
   `TradeChartSeries` on the main pane.
4. Takes a real `EMA` indicator from `FxpaIndicatorManager`, initializes it
   against the same provider, calculates it and binds it as an
   `IndicatorChartSeries`.
5. Renders the pane three times (empty, with candles, with candles + EMA)
   and asserts: series counts, value counts, that candles actually changed
   pixels versus the empty pane, and that the EMA overlay changed them
   again.

`FXPA_UI_SMOKE_DUMP=<dir>` writes the three renders as PNGs so a human can
look at what was painted.

## Why no UIA / mouse driving

Invoking a WinForms handler through a UIA pattern executes the handler on
the UIA RPC thread, which deadlocks against the UI thread in this codebase.
Real input synthesis would avoid that, but it is fragile (screen geometry,
focus stealing, dialog positions) and has no place in an automated gate.
The in-process approach sidesteps the problem completely: no OS-level input
is involved at all, and the scenario is deterministic.

## Scope and limitations

- Covers: chart construction, data binding, indicator wiring, rendering
  produces non-blank candle/indicator pixels, clean teardown.
- Does not cover: menus and dialogs, mouse interaction, layout at other
  DPI/ resolutions, the network or portfolio layers.
- Requires an interactive desktop session (a headless CI runner without a
  session will fail to create the control; that is the intended signal).
- The whole scenario runs under a hard 60-second budget so a regression
  that hangs rendering fails instead of blocking the runner.

## Files

| File | Purpose |
|---|---|
| `UiSmokeTests.cs` | The scenario and its assertions. |
| `StaTestHost.cs` | STA-thread + message-pump harness with timeout. |
| `FixtureDataProvider.cs` | `IDataProvider` over the loaded fixture. |
| `BundledFixture.cs` | Fixture path resolution and SHA-256 pinning. |
| `PixelProbe.cs` | Bitmap statistics (dominant color, changed-pixel count). |
