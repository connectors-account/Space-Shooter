param(
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.0.62f1\Editor\Unity.exe"
)
$ErrorActionPreference = "Stop"
$Project = $PSScriptRoot
if (-not (Test-Path $UnityPath)) {
    throw "Unity 6000.0.62f1 was not found. Install it in Unity Hub, or pass -UnityPath with the full path to Unity.exe."
}
$LogDirectory = Join-Path $Project "Logs"
New-Item -ItemType Directory -Force -Path $LogDirectory | Out-Null
$SetupLog = Join-Path $LogDirectory "setup.log"
$BuildLog = Join-Path $LogDirectory "windows-build.log"
function Invoke-Unity([string]$Method, [string]$LogPath) {
    $Arguments = @("-batchmode", "-quit", "-projectPath", "`"$Project`"", "-buildTarget", "Win64", "-executeMethod", $Method, "-logFile", "`"$LogPath`"")
    $Process = Start-Process -FilePath $UnityPath -ArgumentList $Arguments -Wait -PassThru
    if ($Process.ExitCode -ne 0) { throw "Unity failed (exit $($Process.ExitCode)). Read $LogPath" }
}
Write-Host "Close this project in the Unity Editor before continuing. Building Windows x64..."
Invoke-Unity "Starfall.Editor.ProjectSetup.Generate" $SetupLog
Invoke-Unity "Starfall.Editor.ProjectSetup.BuildWindows" $BuildLog
$Executable = Join-Path $Project "Builds\Windows\Starfall.exe"
if (-not (Test-Path $Executable)) { throw "Unity returned without producing Starfall.exe. Read $BuildLog" }
Write-Host "Build created: $Executable"
Write-Host "Distribute the entire Builds\Windows folder, including its DLLs and Starfall_Data."
