<#
.SYNOPSIS
Builds the QuickTestr NuGet package and optionally publishes it.

.DESCRIPTION
Runs the QuickTestr tests (unless skipped), packs QuickTestr, and writes the
package below artifacts/packages/<version> by default. Publishing is opt-in
and reads NUGET_API_KEY from the process environment or from a .env file in
the QuickTestr or containing QuickCheckr repository root.

.PARAMETER Configuration
The build configuration passed to dotnet test and dotnet pack.

.PARAMETER Version
The package version to create. When omitted, the version is read from
QuickTestr.csproj. Supplying this value does not edit the project file.

.PARAMETER OutputPath
The package output directory. Relative paths are resolved from the QuickTestr
root.

.PARAMETER Publish
Pushes the created package to Source. Without this switch, the script only
builds the package.

.PARAMETER SkipTests
Skips the QuickTestr test run. Intended for cases where the same sources were
already tested.

.PARAMETER Source
The NuGet feed used when Publish is specified.

.EXAMPLE
./Scripts/package.ps1

Tests and packs the version declared in QuickTestr.csproj.

.EXAMPLE
./Scripts/package.ps1 -Version 0.2.0 -OutputPath ./artifacts/release

Tests and creates version 0.2.0 without publishing it.

.EXAMPLE
$env:NUGET_API_KEY = "<key>"
./Scripts/package.ps1 -Version 0.2.0 -Publish

Tests, packs, and publishes version 0.2.0 to nuget.org.
#>
[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release",

    [string] $Version,

    [string] $OutputPath,

    [switch] $Publish,

    [switch] $SkipTests,

    [string] $Source = "https://api.nuget.org/v3/index.json"
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
        throw "dotnet failed with exit code $LASTEXITCODE."
    }
}

function Get-DotEnvValue {
    param(
        [Parameter(Mandatory)]
        [string] $Path,

        [Parameter(Mandatory)]
        [string] $Name
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        return $null
    }

    foreach ($line in Get-Content -LiteralPath $Path) {
        if ($line -match "^\s*$([regex]::Escape($Name))\s*=\s*(.*)\s*$") {
            $value = $Matches[1].Trim()
            if ($value.Length -ge 2) {
                $quotedWithDoubleQuotes = $value.StartsWith('"') -and $value.EndsWith('"')
                $quotedWithSingleQuotes = $value.StartsWith("'") -and $value.EndsWith("'")
                if ($quotedWithDoubleQuotes -or $quotedWithSingleQuotes) {
                    $value = $value.Substring(1, $value.Length - 2)
                }
            }

            return $value
        }
    }

    return $null
}

$quickTestrRoot = Split-Path -Parent $PSScriptRoot
$packageProject = Join-Path $quickTestrRoot "QuickTestr/QuickTestr.csproj"
$testProject = Join-Path $quickTestrRoot "QuickTestr.Tests/QuickTestr.Tests.csproj"

[xml] $projectXml = Get-Content -LiteralPath $packageProject -Raw
$declaredVersion = [string] $projectXml.Project.PropertyGroup.Version
if ([string]::IsNullOrWhiteSpace($declaredVersion)) {
    throw "No <Version> was found in QuickTestr/QuickTestr.csproj."
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = $declaredVersion
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $quickTestrRoot "artifacts/packages/$Version"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath = Join-Path $quickTestrRoot $OutputPath
}

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
$OutputPath = (Resolve-Path -LiteralPath $OutputPath).Path

if (-not $SkipTests) {
    Invoke-DotNet -Arguments @(
        "test", $testProject,
        "--configuration", $Configuration
    )
}

Invoke-DotNet -Arguments @(
    "pack", $packageProject,
    "--configuration", $Configuration,
    "--output", $OutputPath,
    "-p:PackageVersion=$Version"
)

$package = Join-Path $OutputPath "QuickTestr.$Version.nupkg"
if (-not (Test-Path -LiteralPath $package)) {
    throw "Expected package was not created: $package"
}

Write-Host "Created QuickTestr package in $OutputPath"

if (-not $Publish) {
    Write-Host "Package was not published. Pass -Publish to push it to $Source."
    return
}

$apiKey = [Environment]::GetEnvironmentVariable("NUGET_API_KEY")
if ([string]::IsNullOrWhiteSpace($apiKey)) {
    foreach ($dotEnvPath in @(
        (Join-Path $quickTestrRoot ".env"),
        (Join-Path (Split-Path -Parent $quickTestrRoot) ".env")
    )) {
        $apiKey = Get-DotEnvValue -Path $dotEnvPath -Name "NUGET_API_KEY"
        if (-not [string]::IsNullOrWhiteSpace($apiKey)) {
            break
        }
    }
}

if ([string]::IsNullOrWhiteSpace($apiKey)) {
    throw "NUGET_API_KEY is not set in the environment or a repository .env file."
}

Invoke-DotNet -Arguments @(
    "nuget", "push", $package,
    "--source", $Source,
    "--api-key", $apiKey,
    "--skip-duplicate"
)

Write-Host "Published QuickTestr $Version to $Source"
