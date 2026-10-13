# Space Shooter

A minimal **Unity/C# Windows desktop game**, not a web app. Fixed orthographic 2D view, coloured rectangular ships and bullets, one enemy type, score and game-over/restart. No external art, sound, prefabs or scene wiring required.

### Open and play on Windows

1. Install **Unity 6.0 LTS, editor 6000.0.62f1**, through Unity Hub (an available newer 6000.0 LTS patch may upgrade the project). Include Windows Build Support if Hub offers it as an optional module. This project uses the built-in renderer and Mono, not URP or IL2CPP.
2. In Hub choose **Projects → Add → Add project from disk**, then select the `space_shooter` folder containing `Assets`, `Packages` and `ProjectSettings`. Open it and wait for import/compilation.
3. Select **Space Shooter → Open Game Scene**, then press **Play** and click inside the Game view. The editor script creates `Assets/Scenes/Game.unity` automatically. The scene intentionally contains no objects until Play; the runtime script creates the camera, ships, bullets and UI.

The game starts immediately. Destroy red ships for **10 points each**. Touching an enemy **or letting one escape below the screen** ends the game. It is an endless score challenge, with no win screen.

| Control | Action |
| --- | --- |
| WASD or arrow keys | Move; diagonal speed is normalized |
| Hold Space | Shoot repeatedly |
| R or Enter after game-over | Restart |
| Esc | Pause/resume |
| Restart / Resume button | Same actions with the mouse |
| Quit button on an overlay | Close the app (stop Play in the editor) |

Losing window focus pauses the game; press Esc or Resume after returning. Legacy keyboard input is enabled in the project. If you change project settings, keep **Active Input Handling = Input Manager (Old)** or **Both**.

### Build a Windows executable

1. On your Windows machine, exit Play mode and choose **Space Shooter → Build Windows x64**.
2. Wait for Unity to finish and confirm its Console reports success. The command uses the generated scene, Windows x64 target, Mono scripting backend and a resizable 960 × 600 window.
3. Open `Builds/Windows/SpaceShooter.exe`. Share the **entire `Builds/Windows` folder**, not just the executable: the `_Data` folder, `UnityPlayer.dll`, Mono runtime files and any other generated dependencies must travel with it.

If Unity reports missing platform support, install the Windows support module for this exact editor version through Hub. The standard Unity 6 build UI is **File → Build Profiles**, but the supplied menu command already specifies the scene and target.

Optional Windows PowerShell command-line build (adjust the editor path if necessary):

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.0.62f1\Editor\Unity.exe' -batchmode -quit -projectPath "$PWD" -executeMethod SpaceShooter.Editor.SpaceShooterSetup.BuildWindows -logFile "$PWD\windows-build.log"
```

Run that command from the project folder with the project closed in the Unity Editor and a valid Unity license available.

### Complete scripts

- `Assets/Scripts/SpaceShooterGame.cs` — runtime bootstrap, coloured shape rendering, camera, keyboard controls, UI, pause/restart.
- `Assets/Scripts/ShooterModel.cs` — movement, spawning, shooting, collision detection, score and game-over rules; independent of Unity.
- `Assets/Editor/SpaceShooterSetup.cs` — automatic scene creation and Windows build menu.
- `Tests/ModelTests.cs` — executable tests for the actual game rules; outside `Assets` so it is not included in the game build.

### Verification boundary

**Executed here:** the real `ShooterModel.cs` compiled with Mono C# (`mcs -warnaserror`) and passed **28 model checks**, including movement, boundaries, normalized diagonals, firing cadence, bullet cleanup, enemy descent, collisions, scoring, slow-frame substeps, game-over freeze, restart, invalid inputs and bounded bullet counts during a long run.

**Not executed here:** Unity Editor import or compilation, rendering/UI/input integration, scene generation inside Unity, Windows build, or Windows executable testing. No `.exe` is supplied. The Unity-facing scripts were source-reviewed, not compiled against Unity assemblies. Their remaining integration risks include editor import/default settings, platform-module availability and engine-specific rendering/input behavior.

On Windows, verify: open the generated scene without Console errors; move/fire; hit an enemy and see the score increase; touch an enemy or let it escape; restart and see a clean state; pause/resume and switch window focus; resize the window; build and repeat in the executable. This is the remaining runtime acceptance check, not a test already performed.

To repeat the engine-independent checks on a machine with Mono installed:

```sh
mcs -warnaserror -out:/tmp/space-shooter-model-tests.exe Assets/Scripts/ShooterModel.cs Tests/ModelTests.cs
mono /tmp/space-shooter-model-tests.exe
```

### Unity references

- [Pinned editor release](https://unity.com/releases/editor/whats-new/6000.0.62f1)
- [Unity 6 BuildPipeline.BuildPlayer](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html)
- [Windows standalone output files](https://docs.unity3d.com/6000.0/Documentation/Manual/WindowsStandaloneBinaries.html)
