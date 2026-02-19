#define AppName "Whisper -By YashasVM"
#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#define AppPublisher "YashasVM"
#define AppExeName "WhisperByYashasVM.exe"

[Setup]
AppId={{6C2C2C44-2E70-4FA2-AB8E-7FA10A06B3CB}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={localappdata}\Programs\WhisperByYashasVM
DefaultGroupName=Whisper -By YashasVM
DisableProgramGroupPage=yes
OutputDir=dist
OutputBaseFilename=Whisper-By-YashasVM-Setup-v{#AppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest

[Files]
Source: "..\src\WhisperByYashasVM\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Whisper -By YashasVM"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\Whisper -By YashasVM"; Filename: "{app}\{#AppExeName}"

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Launch Whisper -By YashasVM"; Flags: nowait postinstall skipifsilent

[Code]
function RunMinimumSpecCheck(): Boolean;
var
  ResultCode: Integer;
  Command: string;
begin
  Command :=
    '$ErrorActionPreference = ''Stop''; ' +
    '$os = [Environment]::OSVersion.Version; ' +
    '$winOk = [Environment]::Is64BitOperatingSystem -and ($os.Major -ge 10); ' +
    '$ramGb = [math]::Round((Get-CimInstance Win32_ComputerSystem).TotalPhysicalMemory / 1GB, 2); ' +
    '$cpuThreads = (Get-CimInstance Win32_Processor | Measure-Object -Property NumberOfLogicalProcessors -Sum).Sum; ' +
    '$disk = Get-CimInstance Win32_LogicalDisk -Filter "DeviceID=''C:''"; ' +
    '$freeGb = [math]::Round($disk.FreeSpace / 1GB, 2); ' +
    '$avx2 = $true; ' +
    'try { $null = [System.Runtime.Intrinsics.X86.Avx2]; $avx2 = [System.Runtime.Intrinsics.X86.Avx2]::IsSupported } catch { $avx2 = $true }; ' +
    'if ($winOk -and $ramGb -ge 8 -and $cpuThreads -ge 4 -and $freeGb -ge 2 -and $avx2) { exit 0 } else { exit 10 }';

  if Exec(ExpandConstant('{cmd}'),
          '/C powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -Command "' + Command + '"',
          '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Result := ResultCode = 0;
  end
  else
  begin
    Result := False;
  end;
end;

function InitializeSetup(): Boolean;
var
  ContinueInstall: Integer;
begin
  if RunMinimumSpecCheck() then
  begin
    Result := True;
    exit;
  end;

  ContinueInstall := MsgBox(
    'Unsupported PC detected.'#13#10#13#10 +
    'Minimum recommended:'#13#10 +
    '- Windows 10/11 64-bit'#13#10 +
    '- 4 CPU threads'#13#10 +
    '- 8 GB RAM'#13#10 +
    '- AVX2 support'#13#10 +
    '- 2 GB free disk'#13#10#13#10 +
    'You can continue, but performance or compatibility is not guaranteed.',
    mbInformation, MB_YESNO);

  Result := ContinueInstall = IDYES;
end;
