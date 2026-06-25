#requires -Version 5.1
<#
.SYNOPSIS
    Builds the Roche SimuLink Windows installer.

.DESCRIPTION
    Publishes the WinForms app as a self-contained, single-folder win-x64 build
    (the .NET runtime is bundled, so target PCs need no separate install), then
    compiles the Inno Setup script into a single Setup .exe.

    Run this on Windows. Requires the .NET 10 SDK and Inno Setup 6 (ISCC.exe).
    Inno Setup: https://jrsoftware.org/isdl.php

.PARAMETER Version
    Product/installer version (default 1.0.3).

.PARAMETER Configuration
    Build configuration (default Release).

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File installer\build-installer.ps1
#>
[CmdletBinding()]
param(
    [string]$Version = "1.0.3",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

# Resolve paths relative to the repo root (this script lives in installer/).
$repoRoot = Split-Path -Parent $PSScriptRoot
$appProject = Join-Path $repoRoot "src\RocheSimuLink.App\RocheSimuLink.App.csproj"
$issScript = Join-Path $PSScriptRoot "RocheSimuLink.iss"
$publishDir = Join-Path $repoRoot "src\RocheSimuLink.App\bin\$Configuration\net10.0-windows\win-x64\publish"

Write-Host "==> Publishing self-contained win-x64 build (v$Version)..." -ForegroundColor Cyan
dotnet publish $appProject `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:Version=$Version `
    -p:PublishSingleFile=false
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed." }

# Sanity check: the app loads HIMv2_1.pdf from next to the exe at startup.
$pdf = Join-Path $publishDir "HIMv2_1.pdf"
if (-not (Test-Path $pdf)) {
    throw "Bundled HIMv2_1.pdf missing from publish output: $pdf"
}
Write-Host "    Publish output OK (HIMv2_1.pdf present)." -ForegroundColor Green

# Locate the Inno Setup compiler.
$iscc = Get-Command ISCC.exe -ErrorAction SilentlyContinue
if (-not $iscc) {
    $candidates = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
    )
    $iscc = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $iscc) {
        throw "ISCC.exe not found. Install Inno Setup 6 from https://jrsoftware.org/isdl.php"
    }
} else {
    $iscc = $iscc.Source
}

Write-Host "==> Compiling installer with $iscc ..." -ForegroundColor Cyan
& $iscc "/DAppVersion=$Version" "/DPublishDir=$publishDir" $issScript
if ($LASTEXITCODE -ne 0) { throw "Inno Setup compilation failed." }

$setupExe = Join-Path $PSScriptRoot "output\RocheSimuLink-Setup-$Version.exe"
Write-Host "==> Installer built: $setupExe" -ForegroundColor Green
