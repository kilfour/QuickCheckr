<#
.SYNOPSIS
Smoke-tests the QuickTestr NuGet package as an external consumer.

.DESCRIPTION
Packs QuickTestr and its local QuickCheckr dependency with disposable
prerelease versions, restores the PackageSmoke console app from that local
source, then builds and runs it. Package and build caches are recreated so a
cached package or stale binary cannot make the test pass accidentally.

.PARAMETER Configuration
The build configuration passed to dotnet pack, build, and run.

.EXAMPLE
./Scripts/test-packages.ps1

Runs the package smoke test using Release builds.

.EXAMPLE
./Scripts/test-packages.ps1 -Configuration Debug

Runs the package smoke test using Debug builds.
#>
[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Invoke-DotNet {
    param(
        [Parameter(Mandatory)]
        [string[]] $Arguments
    )

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

$quickTestrRoot = Split-Path -Parent $PSScriptRoot
$quickCheckrRoot = Split-Path -Parent $quickTestrRoot
$smokeRoot = Join-Path $quickTestrRoot "artifacts/package-smoke"
$packageSource = Join-Path $smokeRoot "packages"
$packageCache = Join-Path $smokeRoot "global-packages"
$consumerRoot = Join-Path $PSScriptRoot "PackageSmoke"
$consumerProject = Join-Path $consumerRoot "PackageSmoke.csproj"
$nugetConfig = Join-Path $consumerRoot "NuGet.Config"
$packageVersion = "0.0.0-smoke"

foreach ($generatedPath in @(
    $smokeRoot,
    (Join-Path $consumerRoot "bin"),
    (Join-Path $consumerRoot "obj")
)) {
    if (Test-Path -LiteralPath $generatedPath) {
        Remove-Item -LiteralPath $generatedPath -Recurse -Force
    }
}
New-Item -ItemType Directory -Path $packageSource | Out-Null

$projects = @(
    (Join-Path $quickCheckrRoot "QuickCheckr/QuickCheckr.csproj"),
    (Join-Path $quickTestrRoot "QuickTestr/QuickTestr.csproj")
)

foreach ($project in $projects) {
    Invoke-DotNet -Arguments @(
        "pack", $project,
        "--configuration", $Configuration,
        "--output", $packageSource,
        "-p:PackageVersion=$packageVersion"
    )
}

Invoke-DotNet -Arguments @(
    "restore", $consumerProject,
    "--force",
    "--configfile", $nugetConfig,
    "--packages", $packageCache
)
Invoke-DotNet -Arguments @(
    "build", $consumerProject,
    "--configuration", $Configuration,
    "--no-restore"
)
Invoke-DotNet -Arguments @(
    "run", "--project", $consumerProject,
    "--configuration", $Configuration,
    "--no-build",
    "--no-restore"
)

Write-Host "QuickTestr package smoke test passed using $packageSource"
