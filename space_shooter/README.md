### Space Shooter — Windows desktop Unity project

A minimal, silent, fixed-camera shooter. Cyan ship, yellow shots, red enemies; built-in cube geometry only. No sprite art, downloaded assets, third-party packages, audio, web app, or inspector wiring.

### Open and play on your Windows computer

1. Install Unity Hub and **Unity 6.0 LTS, editor 6000.0.60f1**, from the [official release page](https://unity.com/releases/editor/whats-new/6000.0.60f1). This is the pinned baseline, not a claim that it is the latest patch. A newer 6000.0 LTS patch can upgrade the project, but has not been checked here.
2. Download the complete `space_shooter` directory. In Unity Hub choose **Projects → Add → Add project from disk**, select that directory (the one containing `Assets`, `Packages`, and `ProjectSettings`), and open it with the selected editor. Do not create a separate URP project or copy only the scripts.
3. Wait for import and script compilation. The editor script creates, saves, opens, and registers `Assets/Scenes/SpaceShooter.unity` automatically. Unity generates its remaining default settings and package lock locally. The scene looks empty outside Play Mode: the camera and shapes are created at runtime.
4. Press **Play**, select the **Game** tab, and click inside it for keyboard focus. If a different scene is open, use **Space Shooter → Open Game Scene** first. This menu asks before discarding unsaved scene edits.

### Controls and rules

| Control | Action |
|---|---|
| WASD or arrow keys | Move; diagonal movement is normalized |
| Hold Space | Shoot repeatedly |
| Escape | Pause / resume |
| R | Restart immediately, including after game-over |
| Game-over button | Restart |

Destroy a red enemy for **10 points**. Colliding with an enemy, or allowing any enemy to escape completely below the screen, ends the game. There is no final win state: survive and increase your score. Losing window focus pauses play; resume with Escape or the button. Close the window to quit. There is intentionally **no sound**.

### Build a Windows x86_64 executable on your Windows computer

1. Exit Play Mode and use **Space Shooter → Open Game Scene** to ensure the scene is registered.
2. Open **File → Build Profiles**. Select **Windows** under Platforms. If required, choose **Add Build Profile**, choose **Windows**, then **Add Build Profile**. Select the Windows profile and choose **Switch Profile** if it is not active.
3. Set **Architecture → Intel 64-bit** (x86_64), not ARM64. In **Scene List**, confirm that `Assets/Scenes/SpaceShooter.unity` is enabled. If the profile overrides the global scene list, add this scene to its list.
4. In **Player Settings → Other Settings → Configuration**, use **Scripting Backend → Mono** and **Active Input Handling → Input Manager (Old)**. Those defaults are included in the project. Accept an editor restart if changing input handling prompts for one. Keep the Built-in Render Pipeline; do not install URP/HDRP. Leave **Create Visual Studio Solution** and **Development Build** off.
5. Click **Build**, choose a new `Builds/Windows` output folder, and save the executable as `SpaceShooter.exe` if prompted. Run it after the build completes. Windows Mono support is normally supplied with the Windows editor; if Unity reports missing platform support, install the support it identifies using Unity Hub. IL2CPP and Visual Studio C++ tooling are not needed for this Mono build.
6. Share the **entire output folder**, including `SpaceShooter.exe`, `UnityPlayer.dll`, `SpaceShooter_Data`, and any other generated files—not the executable alone.

Official Unity 6.0 references: [Build Profiles](https://docs.unity3d.com/6000.0/Documentation/Manual/build-profiles.html), [Windows build settings](https://docs.unity3d.com/6000.0/Documentation/Manual/WindowsStandaloneBinaries.html).

### Complete source map

- `Assets/Scripts/ShooterModel.cs`: pure C# movement, spawning, swept rectangle collisions, score, and restart state.
- `Assets/Scripts/SpaceShooterGame.cs`: automatic runtime bootstrap, keyboard input, cube visuals, fixed camera, HUD, pause, and game-over controls.
- `Assets/Editor/SpaceShooterSetup.cs`: first-import scene generation and the Open Game Scene menu. Excluded from player builds automatically by its `Editor` folder.
- `Assets/Resources/FlatColor.shader`: small unlit solid-colour shader for the Built-in Render Pipeline. Kept in Resources so player builds retain it.
- `Assets/link.xml`: keeps the engine types used by runtime cube creation from being stripped during builds.
- `Packages/manifest.json`: only Unity's built-in IMGUI, legacy input, physics, and text-rendering modules. Physics is included for `CreatePrimitive`'s generated collider; colliders are disabled/removed and gameplay uses its own swept tests.
- `ProjectSettings/`: editor version, window defaults, Mono, and legacy-input configuration.
- `Tests/ModelTests.cs`: standalone rules tests, outside Assets so they never enter the game build.

The simulation uses 1/120-second ticks. All movement is timestep-scaled; shots and enemies are removed after hits, shots are cleaned up offscreen, and restart clears entity visuals and state. The camera preserves a 16:9 playfield when the window is resized. Focus loss freezes gameplay. No tags, prefabs, input axes, Rigidbody components, or inspector references need configuring.

### Verification and boundary

**Confirmed on the Linux development machine:** the pure C# model and test program compiled with Mono (`mcs -warnaserror`), and **24 checks passed**. All four C# files passed tree-sitter syntax parsing. The package manifest, shader resource path, and legacy-input configuration passed source checks.

**Not verified:** Unity import/compilation of the Unity-dependent scripts, shader compilation, generated scene behaviour, visual output, keyboard/UI interaction in Play Mode, or a Windows build/executable. Unity was not installed or run. Syntax checks and model tests are not substitutes for those checks. No executable is supplied.

For the first Windows check: Play, move in all directions, hold Space, confirm kills add 10 points, let an enemy escape, restart with R and the button, pause/resume, change window focus, and then repeat in the built executable. Check Unity's Console for errors before building.

Optional model test command, from the project root on a machine with Mono installed:

```sh
mcs -warnaserror -out:ModelTests.exe Assets/Scripts/ShooterModel.cs Tests/ModelTests.cs
mono ModelTests.exe
```

These commands test only the model; Unity Hub, Unity Editor, and building the Windows player do not require Mono to be installed separately.
