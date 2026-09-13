<#
.SYNOPSIS
Smoke-tests the QuickCheckr NuGet package as an external consumer.

.DESCRIPTION
Packs QuickCheckr with a disposable prerelease version, restores the
PackageSmoke console app from that local package, then builds and runs it.
The package source, NuGet package cache, and consumer build outputs are
recreated so cached packages or stale binaries cannot make the test pass
accidentally. Dependencies are restored from nuget.org.

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

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$smokeRoot = Join-Path $repositoryRoot "artifacts/package-smoke"
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
    "QuickCheckr/QuickCheckr.csproj"
)

foreach ($project in $projects) {
    Invoke-DotNet -Arguments @(
        "pack",
        (Join-Path $repositoryRoot $project),
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

Write-Host "Package smoke test passed using $packageSource"
