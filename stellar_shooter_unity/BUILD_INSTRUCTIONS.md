### Build Stellar Shooter for Windows x64

**Run these steps on your Windows PC**, not on the Linux authoring machine. No Windows executable has been built or tested here.

1. Install and activate **Unity 6000.0.62f1** in Unity Hub, with Windows player support. Use the built-in rendering pipeline and the supplied packages/settings. This project's helper selects **Mono**, so IL2CPP and Visual Studio C++ tools are not required.
2. Open the project folder (the one directly containing `Assets`, `Packages`, and `ProjectSettings`). Wait for import and script compilation.
3. Choose **Stellar Shooter → 1 - Generate Project**, then **2 - Validate Generated Project**. Generation creates the scene and ten prefabs with all references assigned. Test in Play mode as described in README.md.
4. Exit Play mode. Choose **Stellar Shooter → 3 - Build Windows x64**. The helper uses the existing scene (generates it only if absent), validates references, selects desktop Player / Windows x64 / Mono, and calls Unity's BuildPipeline. A failed build is reported as an error.
5. In the project's `Builds/Windows` folder, run **StellarShooter.exe**. Verify controls, collisions, UI, sound, pause/restart and saved settings in the executable.

Distribute **the entire `Builds/Windows` folder**, including `StellarShooter_Data`, `UnityPlayer.dll`, `MonoBleedingEdge` and any other Unity-generated files. The `.exe` alone will not run. End users do not need Unity Hub. Target a compatible Windows 10/11 64-bit desktop with working graphics/audio drivers. This is an unsigned build; code-sign public releases if needed, rather than disabling Windows security.

Manual alternative: **File → Build Profiles → Windows**, desktop Player, **Intel 64-bit**, Mono in Player settings, and `Assets/StellarShooter/Generated/Scenes/StellarShooter.unity` enabled in the scene list. The provided menu avoids accidental server/ARM64 selection.

### Optional unattended build — Windows PowerShell

Close any Unity instance using this project. Adjust `$project` to your extracted folder. `Start-Process -Wait` is important: it waits for the Windows GUI executable to exit before checking its result.

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.0.62f1\Editor\Unity.exe'
$project = 'C:\UnityProjects\Space-Shooter\stellar_shooter_unity'
if (-not (Test-Path $unity)) { throw 'Install the pinned Unity Editor first.' }
if (-not (Test-Path "$project\Assets")) { throw 'Select the folder containing Assets.' }

foreach ($method in @('Generate', 'BuildWindows')) {
    $log = Join-Path $project "$method.log"
    $arguments = @('-batchmode', '-quit', '-projectPath', "`"$project`"",
        '-buildTarget', 'Win64', '-executeMethod',
        "StellarShooter.Editor.ProjectBuilder.$method", '-logFile', "`"$log`"")
    $process = Start-Process -FilePath $unity -ArgumentList $arguments -Wait -PassThru
    if ($process.ExitCode -ne 0) { throw "$method failed; inspect $log" }
}
if (-not (Test-Path "$project\Builds\Windows\StellarShooter.exe")) {
    throw 'Expected executable was not created; inspect BuildWindows.log.'
}
```

An activated eligible Unity license and Windows target support are required in batch mode too. The `Generate` method intentionally replaces generated scene/prefab edits. These Windows commands have not been executed in the Linux authoring environment.
