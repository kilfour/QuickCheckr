<#
.SYNOPSIS
Builds QuickCheckr NuGet packages and optionally publishes them.

.DESCRIPTION
Runs the solution tests (unless skipped), packs every project listed by this
script, and writes the packages below artifacts/packages/<version> by default.
Publishing is opt-in and reads NUGET_API_KEY from the process environment or
from a .env file in the repository root.

.PARAMETER Configuration
The build configuration passed to dotnet test and dotnet pack.

.PARAMETER Version
The package version to create. When omitted, the version is read from the
package project. Supplying this value does not edit the project file.

.PARAMETER OutputPath
The package output directory. Relative paths are resolved from the repository
root.

.PARAMETER Publish
Pushes the created packages to Source. Without this switch, the script only
builds the packages.

.PARAMETER SkipTests
Skips the solution test run. Intended for cases where the same sources were
already tested.

.PARAMETER Source
The NuGet feed used when Publish is specified.

.EXAMPLE
./Scripts/package.ps1

Tests and packs the version declared in QuickCheckr.csproj.

.EXAMPLE
./Scripts/package.ps1 -Version 0.3.0 -OutputPath ./artifacts/release

Tests and creates version 0.3.0 without publishing it.

.EXAMPLE
$env:NUGET_API_KEY = "<key>"
./Scripts/package.ps1 -Version 0.3.0 -Publish

Tests, packs, and publishes version 0.3.0 to nuget.org.
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

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $repositoryRoot "QuickCheckr.sln"
$projects = @(
    @{ Id = "QuickCheckr"; Path = "QuickCheckr/QuickCheckr.csproj" }
)

$declaredVersions = @(
    foreach ($project in $projects) {
        $projectPath = Join-Path $repositoryRoot $project.Path
        [xml] $projectXml = Get-Content -LiteralPath $projectPath -Raw
        $projectVersion = [string] $projectXml.Project.PropertyGroup.Version
        if ([string]::IsNullOrWhiteSpace($projectVersion)) {
            throw "No <Version> was found in $($project.Path)."
        }

        $projectVersion
    }
)

if ([string]::IsNullOrWhiteSpace($Version)) {
    $uniqueVersions = @($declaredVersions | Sort-Object -Unique)
    if ($uniqueVersions.Count -ne 1) {
        throw "Package project versions do not match: $($uniqueVersions -join ', ')."
    }

    $Version = $uniqueVersions[0]
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repositoryRoot "artifacts/packages/$Version"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath = Join-Path $repositoryRoot $OutputPath
}

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
$OutputPath = (Resolve-Path -LiteralPath $OutputPath).Path

if (-not $SkipTests) {
    Invoke-DotNet -Arguments @(
        "test", $solution,
        "--configuration", $Configuration
    )
}

foreach ($project in $projects) {
    Invoke-DotNet -Arguments @(
        "pack", (Join-Path $repositoryRoot $project.Path),
        "--configuration", $Configuration,
        "--output", $OutputPath,
        "-p:PackageVersion=$Version"
    )
}

$packages = @(
    foreach ($project in $projects) {
        $packagePath = Join-Path $OutputPath "$($project.Id).$Version.nupkg"
        if (-not (Test-Path -LiteralPath $packagePath)) {
            throw "Expected package was not created: $packagePath"
        }

        $packagePath
    }
)

Write-Host "Created $($packages.Count) packages in $OutputPath"

if (-not $Publish) {
    Write-Host "Packages were not published. Pass -Publish to push them to $Source."
    return
}

$apiKey = [Environment]::GetEnvironmentVariable("NUGET_API_KEY")
if ([string]::IsNullOrWhiteSpace($apiKey)) {
    $apiKey = Get-DotEnvValue -Path (Join-Path $repositoryRoot ".env") -Name "NUGET_API_KEY"
}

if ([string]::IsNullOrWhiteSpace($apiKey)) {
    throw "NUGET_API_KEY is not set in the environment or the repository .env file."
}

foreach ($package in $packages) {
    Invoke-DotNet -Arguments @(
        "nuget", "push", $package,
        "--source", $Source,
        "--api-key", $apiKey,
        "--skip-duplicate"
    )
}

Write-Host "Published $($packages.Count) packages to $Source"
