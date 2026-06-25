#requires -Version 5.1
<#
.SYNOPSIS
    Creates a self-signed code-signing certificate for Roche SimuLink.

.DESCRIPTION
    Generates a self-signed code-signing certificate whose subject is
    "Roche Diagnostics International", then exports two files:

      * <OutDir>\RocheSimuLink-CodeSigning.pfx  (private key, password-protected)
            Used to SIGN the app and installer in build-installer.ps1.
            Keep this file and its password secret.

      * <OutDir>\RocheSimuLink-CodeSigning.cer  (public certificate, no key)
            Install this on each target PC's "Trusted Root Certification
            Authorities" (and "Trusted Publishers") store so Windows trusts
            the signature and stops showing "Unknown publisher".

    IMPORTANT — what self-signing does and does not do:
      * It makes the publisher name appear and removes the warning ONLY on
        machines where the .cer above has been installed as trusted.
      * On any other PC, Windows/SmartScreen will still warn, because a
        self-signed certificate is not chained to a public, trusted CA.
      * Practical for a controlled set of lab PCs (push the .cer via Group
        Policy or install it manually). Not suitable for public distribution.

    Run this on Windows in an ELEVATED PowerShell if you intend to import into
    LocalMachine stores; CurrentUser export below does not require elevation.

.PARAMETER OutDir
    Folder to write the .pfx and .cer into (default: installer\certs).

.PARAMETER Password
    Password used to protect the exported .pfx. If omitted, you will be
    prompted securely.

.PARAMETER Subject
    Certificate subject. Defaults to the product publisher name.

.PARAMETER YearsValid
    Certificate validity in years (default 3).

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File installer\create-self-signed-cert.ps1

.EXAMPLE
    # Non-interactive (e.g. for scripting):
    $pw = ConvertTo-SecureString 'MyStrongPass!' -AsPlainText -Force
    .\installer\create-self-signed-cert.ps1 -Password $pw
#>
[CmdletBinding()]
param(
    [string]$OutDir = (Join-Path $PSScriptRoot "certs"),
    [System.Security.SecureString]$Password,
    [string]$Subject = "Roche Diagnostics International",
    [int]$YearsValid = 3
)

$ErrorActionPreference = "Stop"

if (-not $Password) {
    $Password = Read-Host -AsSecureString "Enter a password to protect the exported .pfx"
}

if (-not (Test-Path $OutDir)) {
    New-Item -ItemType Directory -Path $OutDir | Out-Null
}

$pfxPath = Join-Path $OutDir "RocheSimuLink-CodeSigning.pfx"
$cerPath = Join-Path $OutDir "RocheSimuLink-CodeSigning.cer"

Write-Host "==> Creating self-signed code-signing certificate ('$Subject')..." -ForegroundColor Cyan
$cert = New-SelfSignedCertificate `
    -Type CodeSigningCert `
    -Subject "CN=$Subject" `
    -KeyUsage DigitalSignature `
    -KeyExportPolicy Exportable `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -HashAlgorithm SHA256 `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -NotAfter (Get-Date).AddYears($YearsValid)

Write-Host "    Thumbprint: $($cert.Thumbprint)" -ForegroundColor Green

Write-Host "==> Exporting private key to $pfxPath ..." -ForegroundColor Cyan
Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $Password | Out-Null

Write-Host "==> Exporting public certificate to $cerPath ..." -ForegroundColor Cyan
Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null

Write-Host ""
Write-Host "Done." -ForegroundColor Green
Write-Host "  Sign with : .\installer\build-installer.ps1 -CertPath '$pfxPath' -CertPassword <secure>"
Write-Host ""
Write-Host "  Trust on each target PC (elevated PowerShell), so the publisher"
Write-Host "  name shows and the warning disappears:" -ForegroundColor Yellow
Write-Host "    Import-Certificate -FilePath '$cerPath' -CertStoreLocation Cert:\LocalMachine\Root"
Write-Host "    Import-Certificate -FilePath '$cerPath' -CertStoreLocation Cert:\LocalMachine\TrustedPublisher"
Write-Host ""
Write-Host "  Keep the .pfx and its password secret. Do NOT commit them." -ForegroundColor Yellow
