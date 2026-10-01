@echo off
setlocal
rem Run on your Windows development PC, with the Unity Editor closed.
set "UNITY=%ProgramFiles%\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe"
if not "%~1"=="" set "UNITY=%~1"
if not exist "%UNITY%" (
  echo Unity was not found at "%UNITY%".
  echo Usage: BuildWindows.cmd "C:\path\to\Unity.exe"
  exit /b 1
)
"%UNITY%" -batchmode -quit -projectPath "%~dp0." -executeMethod ProjectSetup.BuildWindows -logFile "%~dp0build-windows.log"
if errorlevel 1 (
  echo Build failed. Read build-windows.log in the project folder.
  exit /b 1
)
echo Build command completed. Check Builds\Windows\NovaGuard.exe and build-windows.log.
echo Distribute the entire Builds\Windows folder, not just the EXE.
endlocal
