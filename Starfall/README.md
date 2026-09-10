### STARFALL — Windows desktop space shooter

A lightweight, single-player **2D vertical shooter for Unity 6 / C#**, with original generated sprites and original synthesized sound effects. This is a native Windows desktop source project, **not a web app**. It uses the built-in renderer, 2D physics, legacy keyboard/mouse input, and IMGUI; no paid assets, online services, or runtime credentials are needed.

**Verification boundary:** The included source and media have been inspected and checked on Linux. Unity is not installed here: **Unity compilation, automatic editor setup, serialized scene/prefab generation, Play mode, the Windows build, and Windows runtime have not been executed or verified. No `.exe` is included.** The scene and prefabs are created from the complete editor source when you import the project into Unity, rather than shipped as untested hand-written Unity YAML. See `Documentation/Validation.md` for the exact checks and the remaining Windows acceptance checklist.

### 1. Install Unity on your Windows PC

1. Install **Unity Hub** from https://unity.com/download and sign in. Activate an appropriate Unity Editor license through Hub if prompted.
2. Install **Unity Editor 6000.0.62f1**. If it is not in Hub's standard version list, visit https://unity.com/releases/editor/whats-new/6000.0.62f1 and use **Install this version with Unity Hub**. The project is pinned to this release; do not choose a different version for the initial import.
3. In Hub's installation options, include **Windows Build Support** when offered. For an already installed editor, use **Installs → 6000.0.62f1 → gear/three-dot menu → Add modules**. On Windows, the Windows Mono target is normally included with the editor; Hub may label the additional module **Windows Build Support (IL2CPP)**. Installing that module also permits later IL2CPP work, but **this project's supplied builder uses Mono**, not IL2CPP. You do not need to change scripting backends or install Android/WebGL support. Visual Studio is optional for editing this Mono project; IL2CPP would require the matching Windows C++ toolchain.
4. Extract the source ZIP to a writable local folder. The project root is the folder containing `Assets`, `Packages`, and `ProjectSettings`. Do not open or build from inside the ZIP.
5. In **Unity Hub → Projects → Add → Add project from disk**, select that root folder and open it with **6000.0.62f1**. Allow the first package resolution and script import to finish. Initial editor/package downloads require internet; the game itself does not.

### 2. Import, generate, and Play

1. After script compilation and asset import, `Assets/SpaceShooter/Editor/ProjectSetup.cs` automatically creates the gameplay scene and prefabs when `Assets/SpaceShooter/Scenes/Starfall.unity` does not yet exist.
2. It imports the **included** PNG/WAV files, configures sprites and audio, creates **nine physics prefabs**, assigns all component references, adds the camera/audio/UI/starfield, saves the scene, and adds it to the build scene list. **No artwork generation, Python, manual component wiring, or external API is needed to play or build.**
3. If the automatic setup has not run, wait for compilation to finish and choose **Starfall → Generate or Rebuild Project Assets**. Check **Window → General → Console** for the completion message or any errors. If the Starfall menu is missing, resolve the first C# compilation error before proceeding; generation cannot run until the editor assembly compiles.
4. Choose **Starfall → Validate Generated Assets**. This checks generated physics prefabs and audio; it does not replace Play-testing.
5. Open `Assets/SpaceShooter/Scenes/Starfall.unity` if needed. Set the **Game** view to **720 × 900** or **4:5** to match the portrait desktop window.
6. Press Unity's **Play** button, click the Game view to give it keyboard focus, and choose **Launch** or press **Enter**.

**Generation and editing:** Automatic setup skips an existing scene so ordinary reimports do not replace your edits. The manual generation menu **overwrites the generated prefabs and scene**; back up custom edits first. The PowerShell build script also explicitly regenerates them. If you have customized the generated scene/prefabs, use the **Build Windows x64** menu directly instead. Unity creates `.meta` files and other default project settings on first import. Keep generated `.meta` files in version control together with any assets/scenes you subsequently edit.

### Controls

| Action | Input |
|---|---|
| Move | WASD or arrow keys |
| Fire continuously | Hold Space or left mouse button |
| Toggle mouse steering | M; the ship follows the pointer at its normal movement speed |
| Pause / resume | Esc or P; also the HUD Pause button |
| Start / retry | Enter; R also retries on the game-over screen |
| Sound on/off | Sound button on title or pause menu; preference persists |
| Restart / title / quit | Menu buttons; Quit stops Play mode in the editor |

Keyboard steering is the default for each new run. Losing window focus pauses an active game. To resume, focus the game and press Esc or use Resume. There are no controller or touch bindings.

### Gameplay and component source

- **PlayerShip.cs:** clamped movement, keyboard/mouse steering, held-fire laser, five hull points, brief damage invulnerability with flashing, health repair and timed power-ups.
- **EnemyShip.cs:** three enemy classes, sine-wave movement, wave-scaled health/speed and firing. Scouts fire aimed shots; gunships fire three-way aimed fans; commanders fire rotating eight-way radial bursts. Gunships begin at wave 2, commanders at wave 3. Commanders are a heavy enemy type, not a separate boss encounter.
- **Projectile.cs:** friendly/enemy targeting layers, trigger collision, swept circle queries to reduce fast-shot tunneling, single-hit consumption and lifetime/off-screen cleanup.
- **GameController.cs:** title/play/pause/game-over states, spawning and wave completion, kill score, sector-clear bonus, enemy-escape damage, power-up drops, restart cleanup and persistent personal best.
- **Pickup.cs:** descending, pulsing pickups with single-use collection and cleanup. Repair restores up to two hull points; rapid fire grants triple shots for 12 seconds; shield absorbs up to three hits for 15 seconds. Recollecting timed power-ups refreshes them rather than stacking duration. Damage invulnerability also applies to contact and escaping-enemy damage.
- **Starfield.cs:** deterministic, looping three-layer parallax background with 100 stars and subtle twinkling; frozen with gameplay when paused.
- **GameAudio.cs:** five one-shot sound cues and persistent mute setting.
- **GameUI.cs:** title, HUD, pause and game-over interfaces, hull bar, wave/score, timed power-up status and keyboard/mouse mode indicator.
- **Editor/ProjectSetup.cs:** complete scene/prefab factory, import configuration, project settings, validation menu and Windows builder. This file is editor-only and is not a runtime dependency in the Windows player.

Destroyed scouts/gunships/commanders award 100/250/500 points. Cleared waves award `100 × wave`. Every third kill guarantees a pickup, with additional random drops; pickup kinds cycle through repair, rapid fire and shield. Enemies that escape damage the player; collisions consume the enemy without kill points or a pickup. Waves continue until the hull reaches zero; this is an endless score-attack game, not a finite campaign. Personal best is saved on game over, menu transitions, restart and the Quit button using Unity `PlayerPrefs`. Abrupt process termination is not a score-save action.

### 3. Build a Windows x64 executable

#### Option A — supplied Unity menu (recommended)

On your **Windows PC**, after import/generation:

1. Exit Play mode and save the scene.
2. Choose **Starfall → Validate Generated Assets**.
3. Choose **Starfall → Build Windows x64**. The builder targets `StandaloneWindows64`, uses the generated Starfall scene, and writes `Builds/Windows/Starfall.exe`. If support is unavailable, install the Windows target support through Hub for this exact editor version and retry.
4. Wait for the Console's **Windows build succeeded** message. If the build fails, read the first Console error; no successful executable is assumed.
5. Run `Builds\Windows\Starfall.exe` in Windows. Follow the acceptance checklist in `Documentation/Validation.md`.

You can also inspect the target through Unity 6's **File → Build Profiles → Windows**. The supplied menu removes the need to manually configure scene lists or architecture.

#### Option B — supplied PowerShell script

On your **Windows PC**, close this project in the Unity Editor first; the same project cannot be open in both interactive and batch editors. Open PowerShell in the extracted project root, then run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Build-Windows.ps1
```

If Unity is installed somewhere else:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Build-Windows.ps1 -UnityPath "D:\Unity\6000.0.62f1\Editor\Unity.exe"
```

The per-process `ExecutionPolicy` option does not change your system-wide policy. The script runs asset generation first and then the Windows builder, waits for each process, checks exit codes and checks the output executable exists. Logs are written to `Logs/setup.log` and `Logs/windows-build.log`. A valid local editor license and installed build support are required. As noted above, this option **rebuilds** the generated scene and prefabs.

**Distribute the entire `Builds/Windows` folder**, not just `Starfall.exe`: Unity's `Starfall_Data` directory, `UnityPlayer.dll`, Mono runtime files and any other generated companion files must stay beside the executable. ZIP that folder for another Windows x64 machine. The source ZIP supplied here is not a built game or installer. Code signing and installer packaging are not included.

### Source layout

```text
Assets/SpaceShooter/
  Art/                     10 ready-to-import RGBA sprites
  Audio/                   5 original PCM WAV effects
  Scripts/                 8 full runtime C# components
  Editor/ProjectSetup.cs   Scene/prefab generation and Windows builder
  Prefabs/                 Created by Unity: Player, Scout, Gunship, Commander,
                           PlayerBullet, EnemyBullet, Repair, RapidFire, Shield
  Scenes/Starfall.unity    Created by Unity on first import or menu generation
ArtSource/                 2 original generated PNG sheets
Documentation/
  AssetPreview.png         Contact sheet of final sprites, not a game screenshot
  Validation.md            Linux checks and Windows acceptance checklist
Packages/manifest.json     Minimal Unity package dependencies
ProjectSettings/           Editor version, player, layers and physics settings
Tools/prepare_assets.py    Offline sprite extraction and original WAV synthesis
Tools/check_assets.py      Offline asset integrity checks and contact sheet
Tools/generate_art.py      Earlier optional provider helper; not needed or invoked
Build-Windows.ps1          Windows batch setup/build entry point
ASSETS.md                  Provenance and transformation details
README.md                  This guide
```

### Optional offline asset maintenance

The prepared assets are already included. Only if you deliberately want to rebuild them from the included source sheets, install Python 3 and Pillow, then run in the project root:

```text
python -m pip install Pillow
python Tools/prepare_assets.py
python Tools/check_assets.py --preview
```

These two tools work offline and do not regenerate artwork. Cropping uses the measured gutter in the actual ship sheet so lower-ship exhausts are preserved. The icon background is converted to smooth transparency; the star is a feathered extraction of the generated laser core. The WAVs are deterministic synthesis, not downloaded samples. Python and Pillow are maintenance tools only and are not needed by Unity or the finished Windows player.

### Troubleshooting and limits

- **Input is unresponsive:** focus the Game view/window; check **Edit → Project Settings → Player → Other Settings → Active Input Handling** is **Input Manager (Old)** or **Both**. The supplied project selects the old manager and reads keys directly; it does not need Input Actions or custom axis definitions.
- **Sprites, audio or prefabs are missing:** verify the extracted `Art` and `Audio` directories are present, exit Play mode, then use the generation and validation menus. No remote download is required.
- **Different Game-view shape:** use 720 × 900 / 4:5. The game targets a fixed portrait window; very narrow arbitrary editor aspect ratios are not supported layouts.
- **Editor package/API/compiler error:** retain the exact first Console error and confirm the pinned editor version. The source was syntax-checked, not compiled against Unity; API/package compatibility and generated serialization remain part of first-import verification.
- **Batch build failure:** inspect the supplied log paths, editor license state, project lock and installed Windows target support. Do not treat a zero-length/missing build as a successful game.

The source archive excludes Unity caches, build outputs, temporary checks/packages, logs, API response files, credentials and Git internals. The separately browsable source directory retains local version history on the authoring computer.
