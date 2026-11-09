### Space Shooter — Unity Windows desktop project

Minimal top-down shooter: cyan player, yellow bullets, red enemies. Flat coloured rectangles only; no sound, downloaded art, prefabs, Inspector wiring, third-party packages, or web components.

### Open and play on your Windows PC

1. Install **Unity Hub** and **Unity Editor 6000.0.62f1 (Unity 6.0 LTS)**. This is a pinned, documented version, not a claim to be the latest release. The [official release page](https://unity.com/releases/editor/whats-new/6000.0.62f1) provides a Unity Hub installation link. Activate an appropriate Unity license if prompted.
2. Download/extract this project. In Hub, use **Projects > Add > Add project from disk**, selecting the `space_shooter` folder containing `Assets`, `Packages`, and `ProjectSettings`. Do not create a separate template project. Open with the pinned Editor version and let import/compilation finish.
3. Choose the project's custom **Space Shooter > Play Game** menu, then click inside the Game view to give it keyboard focus.

On first import, the editor script creates `Assets/Scenes/Main.unity` and adds it to the build scene list. The runtime script creates the camera, shapes, and HUD. Unity generates remaining default project settings and metadata. No manual scene editing is required.

**Controls:** WASD or arrow keys move; hold Space to shoot. Destroying an enemy earns **10 points**. Contact with an enemy, or letting one pass the bottom, ends the game. Click **Restart**, or press Enter/R on game over. Esc quits the built app (not the Unity Editor). Losing focus suspends gameplay.

### Build the Windows executable on your Windows PC

1. Stop Play mode.
2. Select **Space Shooter > Build Windows x64**. This custom command selects Windows x64, the **Mono** scripting backend, a resizable 960×720 window, and the generated Main scene. No Visual Studio/C++ toolchain is required for this Mono build.
3. Wait for **“Windows build succeeded”** in Unity's Console. Run `Builds/Windows/SpaceShooter.exe` from the project folder.
4. To share the game, copy/zip the **entire `Builds/Windows` folder**, not only the `.exe`; its data folder, UnityPlayer DLL, Mono runtime, and other emitted files must stay together.

If Unity reports Windows build support unavailable, install/open the matching Windows Editor via Hub and rerun the command. Unity's own build window in this version is **File > Build Profiles** (not the older “Build Settings” name); the custom command above avoids manual profile/scene configuration.

Optional PowerShell command, run on your Windows PC after opening the project once (adjust installation/project paths):

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.0.62f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Projects\space_shooter' -executeMethod SpaceShooter.Editor.SpaceShooterSetup.BuildWindows -logFile 'C:\Projects\space_shooter\build.log'
```

### Complete source

- `Assets/Scripts/ShooterModel.cs` — movement, firing, spawning, swept rectangle collision detection, score, game-over, restart. Collisions are calculated directly; no Rigidbody/Collider setup is needed. Bullet hits resolve before player hits within each update.
- `Assets/Scripts/SpaceShooterGame.cs` — runtime scene construction, keyboard input, coloured shapes, camera, HUD, restart button.
- `Assets/Editor/SpaceShooterSetup.cs` — automatic scene creation and the two custom editor menus.
- `Tests/ModelTests.cs` — executable pure-C# simulation tests, outside Assets so they are not included in the game.

The included settings select the built-in render pipeline and legacy Input Manager; the game reads keys directly, not configured input axes. Keep **Active Input Handling** set to **Input Manager (Old)** or **Both** if changing Player settings. No Input System package is required.

### Verification and limitations

On the Linux authoring machine, all four C# files passed a syntax-parser check and the package manifest parsed as valid JSON. The simulation and test runner compiled with Mono C# with warnings treated as errors, and **24 simulation checks passed**. They cover movement in four directions, diagonal normalization, stopping, bounds, firing/cooldown, spawning, downward motion, swept hits, misses, duplicate-hit scoring, both loss conditions, freeze on game-over, restart, bullet cleanup, and timestep guards.

**Not verified:** Unity Editor import/compilation, actual keyboard/UI/rendering behavior, Windows build, and the Windows executable. No executable is included. The remaining validation must happen in Unity on your Windows PC: play, move/shoot, score a hit, lose by contact and by escape, restart by button and keyboard, then build and repeat in the executable. The source tests do not substitute for that integration check.

To rerun the simulation tests on Linux with Mono installed, from this project's root:

```sh
mcs -warnaserror -out:/tmp/space-shooter-model-tests.exe Assets/Scripts/ShooterModel.cs Tests/ModelTests.cs
mono /tmp/space-shooter-model-tests.exe
```

Verified against official Unity 6.0 documentation: [build window](https://docs.unity3d.com/6000.0/Documentation/Manual/BuildSettings.html), [build API](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html), [scripting backend API](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PlayerSettings.SetScriptingBackend.html), [Windows requirements](https://docs.unity3d.com/6000.0/Documentation/Manual/windows-requirements-and-compatibility.html).
