# Third-party notices

FXPA itself is licensed under the MIT License (see [LICENSE](LICENSE)). The
following third-party components are redistributed with, or used by, this
project.

---

## Bundled binary

### TA-Lib-Core.dll (`src/lib/TA-Lib-Core.dll`)

The .NET port of the TA-Lib technical-analysis library
(`TicTacTec.TA.Library`), used to compute the indicator catalog (RSI,
Bollinger Bands, ADX, MACD, ATR, …).

- Upstream: <https://ta-lib.org/>
- The original TA-Lib C library is distributed under a BSD-style license; the
  .NET port ships under its own permissive terms.
- Redistributed unmodified from the original FXPA prototype.

> If you replace this assembly with a NuGet package, drop the bundled DLL and
> update the reference in `src/fxpa/fxpa.csproj`.

---

## Source incorporated into the project

---

## NuGet dependencies

| Package | Version | License |
|---|---|---|
| [SharpZipLib](https://www.nuget.org/packages/SharpZipLib) | 1.4.2 | MIT |
| [System.Management](https://www.nuget.org/packages/System.Management) | 9.0.0 | MIT |

Resolved at restore time; see `obj/project.assets.json` for the full
transitive graph.

---

## Platform

The application targets **.NET 10** and **Windows Forms**.

- .NET runtime and libraries — MIT (Microsoft)
- Windows Forms — MIT (Microsoft)

---

## Removed legacy components

The following were present in the 2008 prototype and have been **removed**
rather than redistributed:

| Component | Reason |
|---|---|
| `IrisSkin2.dll` (Codejock / Sunisoft) | Commercial library, not licensed for redistribution. Referenced only by legacy, non-compiled code. |
| `default.ssk` | IrisSkin skin file belonging to the above. |
| `FLASH.swf` | Adobe Flash splash asset. Flash is end-of-life; the file was unreferenced. |
| `backup.rar` | Opaque 2008-era binary archive, unreferenced. |
| `ICSharpCode.SharpZipLib.dll` | Stale duplicate of the NuGet package actually referenced by the build. |
| `Net/`, `Handler/`, login & payment forms | The legacy account/licensing client layer — not part of this open-source edition (see README). |
| `Common/Security/` (Blowfish ECB/CBC, Markus Hahn) | Part of the same legacy licensing layer; unused by the build. zlib-licensed, so reusable if ever needed. |
| Uncompiled indicator drafts, legacy UI helpers | Never referenced by the build; superseded by the TA-Lib catalog or the live equivalents. |

---

## Market data

`src/fxpa/fxd/` contains historical GBPUSD minute bars (2010) bundled with the
original prototype and used by the offline simulation data provider. The data
is provided for demonstration and testing purposes.
