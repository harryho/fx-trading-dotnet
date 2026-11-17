# Performance baselines (L6)

A BenchmarkDotNet project that records how long the hot paths take, so a
refactor can be compared against measured numbers instead of vibes.

It is deliberately **not** wired into `fx-trading-dotnet.sln` and never runs
as part of `dotnet test` - a slow or noisy machine must not be able to turn
the unit-test run red. Run it on demand:

```
dotnet run -c Release --project tests/fxpa.benchmarks -- --filter * --job short
```

The repository pins its SDK via `global.json`; the benchmarks spawn child
processes, so on machines with several SDKs pass the CLI explicitly:

```
dotnet run -c Release --project tests/fxpa.benchmarks -- --filter * --job short \
    --cli <path-to-dotnet.exe>
```

## What is measured

| Benchmark | Measures |
|---|---|
| `TaLibRsiBenchmarks.Rsi` | The raw TA-Lib `Core.Rsi` call over a seeded 5,000-price series - the floor cost of any indicator calculation. |
| `FxpaIndicatorBenchmarks.MoneyFlowIndex` | The application's `MFI` indicator class end to end (provider fetch, wrapper overhead, crossing/extremum analysis). |
| `FxpaIndicatorBenchmarks.ExponentialMovingAverage` | The application's `EMA` indicator class end to end. |
| `ChartSeriesScanBenchmarks.TotalMinimum/TotalMaximum` | The min/max scan `ChartPane` performs when fitting the Y axis, over a 5,000-value series. |
| `DataLoadBenchmarks.LoadCdlFile` | `FileWorker.LoadCdlFile` over the bundled GBPUSD MIN1 fixture (5,320 lines / 3,300 bars). |

## Recorded baselines

Measured 2026-11-17 on Windows 11, Ryzen 7 8845HS, .NET 10.0.9, `--job short`
(short runs trade precision for speed; expect a few percent of noise between
runs, more on laptops under battery management).

| Benchmark | Mean | Allocated |
|---|---:|---:|
| TaLibRsiBenchmarks.Rsi (5,000) | 62.8 us | - |
| FxpaIndicatorBenchmarks.MoneyFlowIndex | 547.0 us | 352.6 KB |
| FxpaIndicatorBenchmarks.ExponentialMovingAverage | 233.7 us | 156.5 KB |
| ChartSeriesScanBenchmarks.TotalMinimum | 5.20 us | - |
| ChartSeriesScanBenchmarks.TotalMaximum | 5.22 us | - |
| DataLoadBenchmarks.LoadCdlFile (3,300 bars) | 17.40 ms | 21.44 MB |

These numbers are recorded on one machine and are meant for **relative
comparison after a change** (is the new loader 2x slower?), not as absolute
guarantees.

## Notable numbers

- An FXPA indicator costs roughly 4x the raw TA-Lib call (547 us vs 63 us
  for RSI-like work) - that is the wrapper, the provider fetches and the
  crossing/extremum analysis.
- `LoadCdlFile` allocates ~21 MB to read a ~0.5 MB file: the parser
  allocates a fresh string per field and swallows per-line exceptions.
  The baseline is recorded so a future parser rewrite can prove its
  improvement.

## Known trap

`src/lib/TA-Lib-Core.dll` is a pre-built binary; BenchmarkDotNet's
optimizations validator would abort because it cannot be rebuilt in
Release. `Program.cs` disables that validator on purpose. The child
processes also need an SDK-capable CLI, hence the `--cli` example above.
