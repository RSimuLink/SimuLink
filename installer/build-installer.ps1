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
    Product/installer version (default 1.0.4).

.PARAMETER Configuration
    Build configuration (default Release).

.PARAMETER CertPath
    Optional path to a code-signing .pfx. When supplied, the app executable and
    the final Setup.exe are Authenticode-signed. Create a self-signed .pfx with
    create-self-signed-cert.ps1. Omit to produce an unsigned build.

.PARAMETER CertPassword
    Password (SecureString) for the .pfx. Required when -CertPath is given; you
    will be prompted if it is omitted.

.PARAMETER CertPublicPath
    Optional path to the PUBLIC certificate (.cer) matching -CertPath. When the
    build is signed, this .cer is embedded in the installer so the wizard can
    offer to trust the publisher on the target PC. Defaults to the .cer sitting
    next to the .pfx (same base name), as produced by create-self-signed-cert.ps1.

.PARAMETER TimestampUrl
    RFC 3161 timestamp server, so signatures stay valid after the certificate
    expires (default: DigiCert's public timestamp service).

.EXAMPLE
    # Unsigned build:
    powershell -ExecutionPolicy Bypass -File installer\build-installer.ps1

.EXAMPLE
    # Signed with a self-signed certificate:
    $pw = Read-Host -AsSecureString "PFX password"
    .\installer\build-installer.ps1 -CertPath .\installer\certs\RocheSimuLink-CodeSigning.pfx -CertPassword $pw
#>
[CmdletBinding()]
param(
    [string]$Version = "1.0.4",
    [string]$Configuration = "Release",
    [string]$CertPath,
    [System.Security.SecureString]$CertPassword,
    [string]$CertPublicPath,
    [string]$TimestampUrl = "http://timestamp.digicert.com"
)

$ErrorActionPreference = "Stop"

# --- Optional code-signing setup -------------------------------------------
$signEnabled = -not [string]::IsNullOrWhiteSpace($CertPath)
$signtool = $null
if ($signEnabled) {
    if (-not (Test-Path $CertPath)) {
        throw "CertPath not found: $CertPath"
    }
    if (-not $CertPassword) {
        $CertPassword = Read-Host -AsSecureString "Enter the .pfx password"
    }

    # Locate signtool.exe (ships with the Windows SDK).
    $signtool = (Get-Command signtool.exe -ErrorAction SilentlyContinue).Source
    if (-not $signtool) {
        $found = Get-ChildItem -Path "${env:ProgramFiles(x86)}\Windows Kits\10\bin" `
            -Recurse -Filter signtool.exe -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match '\\x64\\' } |
            Sort-Object FullName -Descending | Select-Object -First 1
        if (-not $found) {
            throw "signtool.exe not found. Install the Windows 10/11 SDK (Signing Tools)."
        }
        $signtool = $found.FullName
    }
    Write-Host "==> Code-signing enabled (signtool: $signtool)" -ForegroundColor Cyan

    # Resolve the public .cer used for the installer's trust step. Default to a
    # .cer next to the .pfx (same base name), as create-self-signed-cert.ps1
    # produces. Optional: if absent, the build is still signed but the installer
    # won't offer the publisher-trust step.
    if ([string]::IsNullOrWhiteSpace($CertPublicPath)) {
        $CertPublicPath = [IO.Path]::ChangeExtension($CertPath, ".cer")
    }
    if (Test-Path $CertPublicPath) {
        Write-Host "    Trust cert for installer: $CertPublicPath" -ForegroundColor Cyan
    } else {
        Write-Host "    No public .cer found ($CertPublicPath); installer will" -ForegroundColor Yellow
        Write-Host "    be signed but won't offer the publisher-trust step." -ForegroundColor Yellow
        $CertPublicPath = $null
    }
}

# Signs one or more files with the configured certificate. No-op when signing
# is disabled, so the same call sites work for unsigned builds.
function Invoke-Sign {
    param([Parameter(Mandatory)][string[]]$Path)
    if (-not $signEnabled) { return }

    $plainPw = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($CertPassword))
    try {
        & $signtool sign /fd SHA256 /f $CertPath /p $plainPw `
            /tr $TimestampUrl /td SHA256 @Path
        if ($LASTEXITCODE -ne 0) { throw "signtool failed for: $($Path -join ', ')" }
    }
    finally {
        $plainPw = $null
    }
}

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

# Sign the app executable before packaging, so the installed app (and its
# Start-menu/desktop shortcuts) carry the publisher identity too.
$appExe = Join-Path $publishDir "RocheSimuLink.App.exe"
if ($signEnabled) {
    Write-Host "==> Signing application executable..." -ForegroundColor Cyan
    Invoke-Sign -Path $appExe
}

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
$isccArgs = @("/DAppVersion=$Version", "/DPublishDir=$publishDir")
# Embed the public cert so the installer can offer the publisher-trust step.
if ($signEnabled -and $CertPublicPath) {
    $isccArgs += "/DCertFile=$CertPublicPath"
}
& $iscc @isccArgs $issScript
if ($LASTEXITCODE -ne 0) { throw "Inno Setup compilation failed." }

$setupExe = Join-Path $PSScriptRoot "output\RocheSimuLink-Setup-$Version.exe"

# Sign the finished installer, so the UAC prompt shown when the user runs it
# carries the publisher identity (on PCs that trust the certificate).
if ($signEnabled) {
    Write-Host "==> Signing installer..." -ForegroundColor Cyan
    Invoke-Sign -Path $setupExe
    if ($CertPublicPath) {
        Write-Host "    Signed. The installer offers a publisher-trust step; once" -ForegroundColor Yellow
        Write-Host "    accepted, the app and future installers show the publisher" -ForegroundColor Yellow
        Write-Host "    on that PC. The very first run on a fresh PC still warns." -ForegroundColor Yellow
    } else {
        Write-Host "    Signed. Publisher shows only on PCs that already trust" -ForegroundColor Yellow
        Write-Host "    this certificate (no .cer embedded for the trust step)." -ForegroundColor Yellow
    }
}

Write-Host "==> Installer built: $setupExe" -ForegroundColor Green
