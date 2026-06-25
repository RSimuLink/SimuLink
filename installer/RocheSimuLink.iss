; Inno Setup script for Roche SimuLink.
;
; Packages the self-contained publish output (app + bundled .NET runtime +
; HIMv2_1.pdf) into a single Windows installer. Because the build is
; self-contained, the target PC needs no separate .NET installation.
;
; Build with: ISCC.exe RocheSimuLink.iss /DPublishDir=<path> /DAppVersion=<ver>
; The build-installer.ps1 script wires these up automatically.

#ifndef AppVersion
  #define AppVersion "1.0.6"
#endif

#ifndef PublishDir
  ; Default to the standard self-contained publish path, relative to this
  ; script's folder (installer/), so ISCC can be run directly after a publish.
  #define PublishDir "..\src\RocheSimuLink.App\bin\Release\net10.0-windows\win-x64\publish"
#endif

; Optional: path to the PUBLIC code-signing certificate (.cer). When defined
; (build-installer.ps1 passes it automatically for signed builds), the wizard
; offers to trust this publisher certificate so Windows shows the publisher
; name and stops warning on this PC for future runs and updates. Self-signed
; certs are not chained to a public CA, so this trust step is what makes the
; signature recognized. Omit for unsigned builds (no trust step is shown).
;   ISCC ... /DCertFile=<path-to-.cer>

#define AppName "Roche SimuLink"
#define AppExe "RocheSimuLink.App.exe"
#define AppPublisher "Roche Diagnostics International"

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
#ifdef CertFile
; Opt-in: import the publisher certificate into the machine trust stores so the
; signed app and future installers show "{#AppPublisher}" instead of an unknown
; publisher. Checked by default; the admin can untick it.
Name: "trustcert"; Description: "Trust the {#AppPublisher} publisher certificate (recommended)"; GroupDescription: "Publisher trust:"
#endif

[Files]
; Recursively include the entire self-contained publish output. This already
; contains RocheSimuLink.App.exe, the .NET runtime, and HIMv2_1.pdf (copied
; next to the exe), which the app loads at startup.
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion
#ifdef CertFile
; Bundle the public certificate so the trust task can import it, and the
; uninstaller can remove it again.
Source: "{#CertFile}"; DestName: "publisher.cer"; DestDir: "{app}"; Tasks: trustcert
#endif

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
#ifdef CertFile
; Import the publisher certificate into the machine Root and TrustedPublisher
; stores (installer runs elevated). After this, the signed app and any future
; installer signed with the same certificate show "{#AppPublisher}" and no
; longer warn on this PC. certutil ships with Windows.
Filename: "certutil.exe"; Parameters: "-f -addstore Root ""{app}\publisher.cer"""; Tasks: trustcert; Flags: runhidden waituntilterminated; StatusMsg: "Trusting publisher certificate..."
Filename: "certutil.exe"; Parameters: "-f -addstore TrustedPublisher ""{app}\publisher.cer"""; Tasks: trustcert; Flags: runhidden waituntilterminated; StatusMsg: "Trusting publisher certificate..."
#endif
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

#ifdef CertFile
[UninstallRun]
; Remove the publisher certificate this installer added (match by subject CN).
; runonce so it isn't re-run; errors ignored in case it was already removed.
Filename: "certutil.exe"; Parameters: "-delstore Root ""{#AppPublisher}"""; Flags: runhidden waituntilterminated runonce; RunOnceId: "DelTrustRoot"
Filename: "certutil.exe"; Parameters: "-delstore TrustedPublisher ""{#AppPublisher}"""; Flags: runhidden waituntilterminated runonce; RunOnceId: "DelTrustPub"
#endif
