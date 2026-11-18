# Measure code coverage and build the HTML report.
#
#   powershell -ExecutionPolicy Bypass -File tools\measure-coverage.ps1
#
# It does the whole job in the right order:
#   1. removes the previous results, so a stale report can never be merged in;
#   2. runs the suite with the coverage collector (the scope comes from
#      coverlet.runsettings, which the test project applies automatically);
#   3. feeds ONLY the newest coverage file to ReportGenerator;
#   4. opens the HTML report.
#
# The coverage scope - the application's own code, without the generated
# designer output and the vendored third-party components - lives in
# coverlet.runsettings at the repository root.

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
Push-Location $root

try {
    Write-Host 'Cleaning previous coverage results...'
    Remove-Item -Recurse -Force 'tests\fxpa.tests\TestResults' -ErrorAction SilentlyContinue
    Remove-Item -Recurse -Force 'coveragereport' -ErrorAction SilentlyContinue

    Write-Host 'Running the test suite with coverage collection...'
    dotnet test fx-trading-dotnet.sln -c Release --collect 'XPlat Code Coverage'
    if ($LASTEXITCODE -ne 0) { throw "The test run failed (exit code $LASTEXITCODE)." }

    $xml = Get-ChildItem 'tests\fxpa.tests\TestResults' -Recurse -Filter 'coverage.cobertura.xml' -ErrorAction SilentlyContinue |
           Sort-Object LastWriteTime -Descending |
           Select-Object -First 1

    if (-not $xml) { throw 'No coverage file was produced.' }

    Write-Host "Coverage file: $($xml.FullName)"
    Write-Host 'Generating the HTML report...'
    reportgenerator "-reports:$($xml.FullName)" -targetdir:coveragereport
    if ($LASTEXITCODE -ne 0) { throw "ReportGenerator failed (exit code $LASTEXITCODE)." }

    $report = Resolve-Path 'coveragereport\index.html'
    Write-Host "Report: $report"
    Start-Process $report
}
finally {
    Pop-Location
}
