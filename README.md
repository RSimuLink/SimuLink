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
from Linux).

### Locally on Windows

Requires the .NET 10 SDK and [Inno Setup 6](https://jrsoftware.org/isdl.php):

```powershell
powershell -ExecutionPolicy Bypass -File installer\build-installer.ps1 -Version 1.0.0
```

This publishes a self-contained `win-x64` build and compiles the installer to
`installer\output\RocheSimuLink-Setup-1.0.0.exe`.

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
