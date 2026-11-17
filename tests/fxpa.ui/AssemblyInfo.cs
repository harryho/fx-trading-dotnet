using Xunit;

// The smoke tests touch process-wide state (fxpa.AppContext.TradeAnalyzerControl)
// and create real windows, so they must not run concurrently with each other.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
