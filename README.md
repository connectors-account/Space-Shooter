### Space Shooter — Unity Windows desktop source

Move the cyan ship, shoot the red enemies, and earn **10 points per enemy**. The round ends when an enemy hits you or escapes below the screen. Restart from the game-over screen. Endless score-based play, built-in shapes and colours, no audio or external assets.

### Open and play

1. On your Windows PC, install **Unity 6.3 LTS — 6000.3.0f1** through Unity Hub. This is the pinned version, not a claim that it is the latest patch.
2. In Unity Hub, add/open this existing `space_shooter` folder, containing `Assets`, `Packages`, and `ProjectSettings`. Do not create a separate URP project.
3. Wait for import/compilation. The editor script automatically creates the Main scene, material assets, and build scene list. Unity generates omitted default project settings and its package lock file on import.
4. Choose **Space Shooter > Setup / Open Main**, press **Play**, and click the Game view for keyboard focus.

The scene is intentionally empty outside Play mode: all game objects and UI are created at runtime. No references, prefabs, input axes, or colliders need wiring.

### Controls

| Key | Action |
| --- | --- |
| WASD / arrows | Move |
| Hold Space | Shoot |
| P | Pause / resume |
| R / Enter after game over | Restart |
| Escape | Quit executable / stop editor Play mode |

Losing window focus pauses the round. Press P or click Resume after returning. Restart also has an on-screen button.

### Build Windows x86_64

**On your Windows PC, in Unity:**

1. Stop Play mode and resolve any Console errors.
2. Choose **Space Shooter > Build Windows x86_64**.
3. The supplied command builds `Builds/Windows/SpaceShooter.exe` using **Mono**, `StandaloneWindows64`, and a resizable 960 × 720 window. Mono avoids the IL2CPP C++ toolchain requirement. If Unity reports missing Windows build support, check your Windows Editor installation in Unity Hub.
4. Run the executable on Windows. Share the **entire `Builds/Windows` folder**, including Unity's DLLs and data folder, not just the `.exe`.

Manual alternative: **File > Build Profiles**, select/add and activate Windows, choose **Architecture: Intel 64-bit**, include Main in the Scene List, set the scripting backend to Mono in Player Settings, then Build. The supplied menu command sets the scene, backend, and target for you.

### Source and configuration

- `Assets/Scripts/SpaceShooterGame.cs`: complete runtime game, including bootstrap, movement, firing, spawning, rectangle/swept-shot collisions, score, game-over, restart, and pause. Change the constants at the top to tune speeds and intervals.
- `Assets/Editor/SpaceShooterSetup.cs`: automatic scene/material creation and editor menus. Excluded from the executable.
- `Assets/link.xml`: preserves the components needed by runtime cube creation during code stripping.
- `Packages/manifest.json`: Unity's built-in IMGUI, legacy input, and physics modules only. Physics supplies the collider type created with primitive cubes; these colliders are removed because collision checks are manual.
- `ProjectSettings`: editor version and initial player settings. **Input Manager (Old)** is explicitly selected (`activeInputHandler: 0`). Input uses `Input.GetKey`, not the new Input System. Keep the built-in render pipeline and legacy input enabled.

Material assets in Resources retain the built-in unlit shader in the player build. No sprite art, sound, third-party packages, network services, or web app are included.

### Validation and limits

Passed here: C# syntax parsing for both scripts, JSON/XML parsing, package/version/input configuration checks, unique asset metadata, source-level scene/material/build-target wiring checks, and Git whitespace checks.

**Unity import/compilation, editor gameplay, Windows executable build, and Windows runtime tests have not been run.** Unity was not installed for this task. Source parsing is not Unity compilation; editor APIs, first-import settings migration, build stripping, and actual rendering/input still need confirmation in the pinned editor.

On Windows, check movement and bounds; held fire; exactly 10 points per enemy destroyed; game-over on contact and escape; score/actor reset on restart; pause/focus loss freezing simulation; window resizing; and the standalone executable's controls and colours.

Official references: [6000.3.0f1 release](https://unity.com/releases/editor/whats-new/6000.3.0f1), [Windows build settings](https://docs.unity3d.com/6000.3/Documentation/Manual/WindowsStandaloneBinaries.html), [scripting backends](https://docs.unity3d.com/6000.3/Documentation/Manual/scripting-backends.html).
