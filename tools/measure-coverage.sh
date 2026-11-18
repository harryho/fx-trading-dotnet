#!/usr/bin/env bash
# Measure code coverage and build the HTML report (WSL / Git Bash).
#
#   ./tools/measure-coverage.sh
#
# It does the whole job in the right order:
#   1. removes the previous results, so a stale report can never be merged in;
#   2. runs the suite with the coverage collector (the scope comes from
#      coverlet.runsettings, which the test project applies automatically);
#   3. feeds ONLY the newest coverage file to ReportGenerator;
#   4. prints the path of the HTML report.
#
# The coverage scope - the application's own code, without the generated
# designer output and the vendored third-party components - lives in
# coverlet.runsettings at the repository root.

set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root"

DOTNET="${DOTNET:-/mnt/c/Users/harry/scoop/apps/dotnet10-sdk/current/dotnet.exe}"
REPORTGEN="${REPORTGEN:-/mnt/c/Users/harry/.dotnet/tools/reportgenerator.exe}"

echo 'Cleaning previous coverage results...'
rm -rf tests/fxpa.tests/TestResults coveragereport

echo 'Running the test suite with coverage collection...'
"$DOTNET" test fx-trading-dotnet.sln -c Release --collect 'XPlat Code Coverage'

xml="$(find tests/fxpa.tests/TestResults -name 'coverage.cobertura.xml' -print -quit)"
if [ -z "$xml" ]; then
    echo 'No coverage file was produced.' >&2
    exit 1
fi

echo "Coverage file: $xml"
echo 'Generating the HTML report...'
"$REPORTGEN" "-reports:$xml" -targetdir:coveragereport

report="$root/coveragereport/index.html"
echo "Report: $report"
[ -f "$report" ] && explorer.exe "$(wslpath -w "$report")" || true
