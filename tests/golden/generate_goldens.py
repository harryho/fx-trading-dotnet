#!/usr/bin/env python3
"""Golden-master generator for the TA-Lib catalog tests.

Regenerates the JSON goldens under Golden/ consumed by
tests/fxpa.tests/CatalogGoldenTests.cs.

The input series is fully deterministic (seeded), so the goldens are
reproducible byte-for-byte. The values come from the canonical C TA-Lib
via the python ta-lib bindings, providing an *independent* reference for
the managed port bundled with this repository (src/lib/TA-Lib-Core.dll).

Usage:
    python3 -m venv .venv && .venv/bin/pip install numpy ta-lib
    .venv/bin/python generate_goldens.py
"""

import json
import os
import random

import numpy as np
import talib

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, "Golden")

COUNT = 512
SEED = 42
STEP_LO, STEP_HI = -1.5, 1.6


def make_inputs():
    """Deterministic price series, identical to what the C# tests consume."""
    rng = random.Random(SEED)
    close = [100.0]
    for _ in range(COUNT - 1):
        close.append(round(close[-1] + rng.uniform(STEP_LO, STEP_HI), 6))
    close = close[:COUNT]

    high, low, volume = [], [], []
    for i, c in enumerate(close):
        high.append(round(c + 0.35 + ((i * 7) % 11) / 100.0, 6))
        low.append(round(c - 0.35 - ((i * 13) % 11) / 100.0, 6))
        volume.append(1000 + ((i * 37) % 501))

    return {
        "close": close,
        "high": high,
        "low": low,
        "volume": volume,
    }


def run(function, params, outputs, arrays):
    """Call talib, return (begIdx, {name: values-from-begIdx})."""
    if not isinstance(arrays, tuple):
        arrays = (arrays,)
    results = {}
    beg_idx = None
    for name, arr in zip(outputs, arrays):
        values = np.asarray(arr, dtype=float)
        first_valid = int(np.argmax(~np.isnan(values)))
        assert not np.isnan(values[first_valid:]).any(), f"{function}/{name}: NaN after warmup"
        results[name] = [float(v) for v in values[first_valid:]]
        beg_idx = first_valid if beg_idx is None else max(beg_idx, first_valid)
    return beg_idx, results


def c(close, high, low, volume):
    return {
        "close": np.array(close, dtype=float),
        "high": np.array(high, dtype=float),
        "low": np.array(low, dtype=float),
        "volume": np.array(volume, dtype=float),
    }


def main():
    os.makedirs(OUT, exist_ok=True)
    src = make_inputs()
    a = c(src["close"], src["high"], src["low"], src["volume"])

    with open(os.path.join(OUT, "inputs.json"), "w") as f:
        json.dump({"count": COUNT, "seed": SEED, **src}, f, indent=1)

    cases = [
        ("SMA", {"optInTimePeriod": 14}, ["outReal"], talib.SMA(a["close"], 14)),
        ("EMA", {"optInTimePeriod": 14}, ["outReal"], talib.EMA(a["close"], 14)),
        ("WMA", {"optInTimePeriod": 14}, ["outReal"], talib.WMA(a["close"], 14)),
        ("DEMA", {"optInTimePeriod": 14}, ["outReal"], talib.DEMA(a["close"], 14)),
        ("TEMA", {"optInTimePeriod": 14}, ["outReal"], talib.TEMA(a["close"], 14)),
        ("RSI", {"optInTimePeriod": 14}, ["outReal"], talib.RSI(a["close"], 14)),
        ("MOM", {"optInTimePeriod": 10}, ["outReal"], talib.MOM(a["close"], 10)),
        ("ROC", {"optInTimePeriod": 10}, ["outReal"], talib.ROC(a["close"], 10)),
        ("MACD", {"optInFastPeriod": 12, "optInSlowPeriod": 26, "optInSignalPeriod": 9},
         ["outMacd", "outSignal", "outHist"], talib.MACD(a["close"], 12, 26, 9)),
        ("BBANDS", {"optInTimePeriod": 20, "optInNbDevUp": 2.0, "optInNbDevDn": 2.0, "optInMAType": 0},
         ["outUpper", "outMiddle", "outLower"], talib.BBANDS(a["close"], 20, 2, 2, 0)),
        ("ATR", {"optInTimePeriod": 14}, ["outReal"], talib.ATR(a["high"], a["low"], a["close"], 14)),
        ("ADX", {"optInTimePeriod": 14}, ["outReal"], talib.ADX(a["high"], a["low"], a["close"], 14)),
        ("CCI", {"optInTimePeriod": 14}, ["outReal"], talib.CCI(a["high"], a["low"], a["close"], 14)),
        ("WILLR", {"optInTimePeriod": 14}, ["outReal"], talib.WILLR(a["high"], a["low"], a["close"], 14)),
        ("STOCH", {"optInFastKPeriod": 14, "optInSlowKPeriod": 3, "optInSlowDPeriod": 3},
         ["outSlowK", "outSlowD"],
         talib.STOCH(a["high"], a["low"], a["close"], 14, 3, 0, 3, 0)),
        ("OBV", {}, ["outReal"], talib.OBV(a["close"], a["volume"])),
    ]

    for function, params, outputs, arrays in cases:
        beg_idx, results = run(function, params, outputs, arrays)
        doc = {
            "function": function,
            "parameters": params,
            "inputs": {"count": COUNT, "seed": SEED},
            "outBegIdx": beg_idx,
            "tolerance": 1e-6,
            "outputs": results,
        }
        path = os.path.join(OUT, f"{function}.json")
        with open(path, "w") as f:
            json.dump(doc, f, indent=1)
        n_out = sum(len(v) for v in results.values())
        print(f"{path}: begIdx={beg_idx} outputs={list(results)} values={n_out}")


if __name__ == "__main__":
    main()
