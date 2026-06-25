# Roche SimuLink

A LIS (Laboratory Information System) connectivity simulator for Roche x800
Data Manager instruments. It sends HL7 v2 result messages to a LIS and handles
the order/query/acknowledgement workflows defined in the instrument's LAW
(Laboratory Automation Workflow) profile, communicating over MLLP.

The assay catalog (tests, sample types, volumes) is derived from a Roche Host
Interface Manual: a bundled manual ships as the default, and a different one can
be imported at runtime.

## Features

- **HL7 v2 messaging** over MLLP, for the LAW profile and generic workflows:

  | Workflow | Message | Purpose |
  | --- | --- | --- |
  | `OUL^R22` | Unsolicited result | Send instrument results to the LIS |
  | `OML^O33` | Laboratory order | Inbound order |
  | `ORL^O34` | Laboratory order response | Order acknowledgement |
  | `QBP^Q11` | Query by parameter | Work-order query |
  | `RSP^K11` | Query response | Query reply |
  | `ACK^R22` | Acknowledgement | Result acknowledgement |

- **Example Generator** (Tools → Example Generator): produces the full set of
  LAB-27/28/29 HL7 example messages from the values entered in the main window,
  **without a LIS connection**. Pick any of the seven flows, generate, then view,
  copy, or save the messages:

  | Workflow | Message | Direction |
  | --- | --- | --- |
  | LAB-27 Work order request | `QBP^Q11` | Instrument → LIS |
  | LAB-27 Request acknowledge | `RSP^K11` | LIS → Instrument |
  | LAB-28 Test order submission | `OML^O33` | LIS → Instrument |
  | LAB-28 Response to a test order | `ORL^O34` | Instrument → LIS |
  | LAB-29 Test result | `OUL^R22` | Instrument → LIS |
  | LAB-29 Result accepted | `ACK^R22` | LIS → Instrument |
  | Control Test result | `OUL^R22` | Instrument → LIS (QC specimen) |

- **Host Interface Manual ingestion**: parses the manual PDF into a structured
  assay catalog (tests, targets, sample types, input volumes) that drives the
  UI dropdowns and message building.
- **Bundled default catalog**: `HIMv2_1.pdf` is shipped with the app and parsed
  at startup, so the simulator works out of the box without an import step.
- **Remembered catalogs**: an imported manual can be persisted to survive
  application restarts.

## Project layout

| Path | Description |
| --- | --- |
| `src/RocheSimuLink.Core` | Platform-agnostic library: HL7 builders/parsers, MLLP transport, HIM ingestion, models, services. |
| `src/RocheSimuLink.App` | WinForms desktop UI (`net10.0-windows`). |
| `tests/RocheSimuLink.Core.Tests` | xUnit test suite for the Core library. |
| `HIMv2_1.pdf` | Host Interface Manual bundled as the default assay catalog. |
| `HIV HL7 Example.pdf` | Reference message trace used by the result-builder tests. |

## Requirements

- .NET SDK 10.0
- The desktop app (`RocheSimuLink.App`) targets `net10.0-windows` and **runs
  only on Windows**. The Core library and tests are platform-agnostic and build
  and run on Linux, macOS, and Windows.

## Build and test

The Core library and test suite build on any platform:

```bash
dotnet restore RocheSimuLink.slnx
dotnet build src/RocheSimuLink.Core/RocheSimuLink.Core.csproj
dotnet test tests/RocheSimuLink.Core.Tests/RocheSimuLink.Core.Tests.csproj
```

> In a Gitpod/Ona environment these are available as the **Install
> dependencies**, **Build**, and **Test** automations (`.ona/automations.yaml`).

### Building the desktop app

On Windows:

```bash
dotnet build src/RocheSimuLink.App/RocheSimuLink.App.csproj
```

On a non-Windows host you can still compile (but not run) the WinForms project
by enabling Windows targeting:

```bash
dotnet build src/RocheSimuLink.App/RocheSimuLink.App.csproj -p:EnableWindowsTargeting=true
```

## Packaging a Windows installer

The app can be packaged as a single self-contained Windows installer. "Self-
contained" means the .NET runtime is bundled, so target machines need **no
separate .NET installation** — install and run on any 64-bit Windows PC.

The installer must be built **on Windows** (the WinForms app cannot be published
from Linux). There are two ways:

### Via GitHub Actions (no Windows machine needed)

The **Windows installer** workflow builds the installer on a Windows runner:

- Run it manually from the Actions tab (provide a version), or
- Push a `v*` tag (e.g. `v1.0.0`) to build and attach the installer to a GitHub
  Release.

The resulting `RocheSimuLink-Setup-<version>.exe` is uploaded as a build
artifact (and release asset for tags).

### Locally on Windows

Requires the .NET 10 SDK and [Inno Setup 6](https://jrsoftware.org/isdl.php):

```powershell
powershell -ExecutionPolicy Bypass -File installer\build-installer.ps1 -Version 1.0.6
```

This publishes a self-contained `win-x64` build and compiles the installer to
`installer\output\RocheSimuLink-Setup-1.0.6.exe`.

### Code signing (optional, self-signed)

By default the installer is **unsigned**, so Windows shows "Unknown publisher"
on the UAC prompt. You can sign it with a **self-signed** certificate to make
the publisher name appear and clear the warning — but only on PCs that are
configured to trust that certificate. This is suitable for a controlled set of
lab machines, **not** for public distribution (for that you need a certificate
from a trusted CA).

1. Create the certificate (once). This writes a `.pfx` (private key, for
   signing) and a `.cer` (public cert, for trusting on target PCs) into
   `installer\certs\`:

   ```powershell
   powershell -ExecutionPolicy Bypass -File installer\create-self-signed-cert.ps1
   ```

2. Build and sign in one step:

   ```powershell
   $pw = Read-Host -AsSecureString "PFX password"
   .\installer\build-installer.ps1 -CertPath .\installer\certs\RocheSimuLink-CodeSigning.pfx -CertPassword $pw
   ```

   This signs both the app executable and the final `Setup.exe`, and embeds the
   public `.cer` (found next to the `.pfx`) so the installer can offer to trust
   the publisher.

3. **Run the installer on the target PC.** When the build is signed, the wizard
   shows a checkbox — *"Trust the Roche Diagnostics International publisher
   certificate (recommended)"* — ticked by default. Leaving it ticked imports
   the certificate into the machine trust stores during install, so afterwards
   the installed app, its shortcuts, and any future installer signed with the
   same certificate show **Roche Diagnostics International** and no longer warn
   on that PC.

   **First-run caveat:** the certificate is trusted *during* installation, so
   the **very first** UAC prompt — shown the moment you launch `Setup.exe` on a
   fresh PC, before anything runs — still says "Unknown publisher." Every prompt
   after that (the installed app, re-running the installer, future updates) shows
   the publisher correctly.

   To fix even that first prompt, pre-trust the certificate before running the
   installer — typically pushed once via **Group Policy** in a managed
   environment, or manually in an elevated PowerShell:

   ```powershell
   Import-Certificate -FilePath RocheSimuLink-CodeSigning.cer -CertStoreLocation Cert:\LocalMachine\Root
   Import-Certificate -FilePath RocheSimuLink-CodeSigning.cer -CertStoreLocation Cert:\LocalMachine\TrustedPublisher
   ```

   A self-signed certificate is not chained to a public CA, so this trust step
   (whether via the installer checkbox or Group Policy) is required on each PC.
   The uninstaller removes the certificate it added.

> The `.pfx`, `.cer`, and `installer\certs\` are git-ignored. Never commit the
> private key or its password.

### Installing

Run the Setup `.exe`. It installs to Program Files, adds a Start-menu entry
(and an optional desktop shortcut), bundles the default `HIMv2_1.pdf` catalog,
and registers an uninstaller in Add/Remove Programs.

## Running the simulator

Launch `RocheSimuLink.App` on Windows. From the **Settings** dialog you can:

- Configure the LIS connection — host and port for outbound results
  (default `127.0.0.1:5000`), and the local listener port for inbound orders
  (default `5001`).
- Set the HL7 identity fields (MSH-3 through MSH-6, MSH-12).
- Load an assay catalog from a Host Interface Manual PDF or a portable
  `HIMdefinitions_*.txt` file, and optionally remember it across restarts.

The main window connects to the LIS, builds and sends results from the loaded
catalog, displays inbound orders, and shows an activity log.

## Assay catalog

On startup the catalog is sourced from the bundled `HIMv2_1.pdf`. If that file
is missing or cannot be parsed, the simulator starts with an empty catalog
rather than failing — a manual can then be imported from Settings. An imported
manual replaces the default for the session, and is persisted across restarts
when "remember catalog" is selected.

> Note: the parser reads each assay's result-code table (OBX-5 values such as
> POS/NEG or VAL/AT/BT/ND, paired with their OBX-8-1 interpretations), so the
> result dropdown is populated for most targets out of the box. A few
> control/blood-screening assays whose code tables span a page boundary are not
> stitched together and have empty values until entered in the UI.
