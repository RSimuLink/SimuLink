; Inno Setup script for Roche SimuLink.
;
; Packages the self-contained publish output (app + bundled .NET runtime +
; HIMv2_1.pdf) into a single Windows installer. Because the build is
; self-contained, the target PC needs no separate .NET installation.
;
; Build with: ISCC.exe RocheSimuLink.iss /DPublishDir=<path> /DAppVersion=<ver>
; The build-installer.ps1 script wires these up automatically.

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

#ifndef PublishDir
  ; Default to the standard self-contained publish path, relative to this
  ; script's folder (installer/), so ISCC can be run directly after a publish.
  #define PublishDir "..\src\RocheSimuLink.App\bin\Release\net10.0-windows\win-x64\publish"
#endif

#define AppName "Roche SimuLink"
#define AppExe "RocheSimuLink.App.exe"
#define AppPublisher "Roche"

[Setup]
AppId={{B6F0D6F2-3C7A-4E2C-9A1E-7E2D4F1B9C10}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
; Per-machine install for all users; installer requests elevation.
PrivilegesRequired=admin
OutputDir=output
OutputBaseFilename=RocheSimuLink-Setup-{#AppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#AppExe}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Recursively include the entire self-contained publish output. This already
; contains RocheSimuLink.App.exe, the .NET runtime, and HIMv2_1.pdf (copied
; next to the exe), which the app loads at startup.
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent
