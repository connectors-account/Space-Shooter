### Orbit Breaker — Unity 2D Windows desktop source

A small single-player arcade space shooter. **This is a native Unity desktop project, not a website.** The deliverable is source, not a Windows executable.

**Target Editor: Unity 2022.3.62f3 (2022 LTS).** Built-in render pipeline, Unity UI (`com.unity.ugui` 1.0.0), legacy Input Manager, Mono scripting backend, .NET Standard 2.1 API profile, Windows x86_64. No paid assets or external gameplay libraries.

**Verification boundary:** C# syntax and asset/source integrity were checked on Linux. No Unity Editor is installed in the delivery environment. Unity compilation, first-open generation, Play mode, collision behavior, audio playback, and Windows build/runtime have **not** been executed. `Validation.json` records exactly the offline checks, not a claim of a working Unity build.

### First open — no manual assembly

1. Install **Unity Hub** and activate an appropriate Unity license. Install **2022.3.62f3** from [its official release page](https://unity.com/releases/editor/whats-new/2022.3.62f3) using **Install in Unity Hub**.
2. Ensure **Windows Standalone build support** is available for this Editor. The Windows Editor includes native Windows/Mono support; on another supported Editor host, install **Windows Build Support (Mono)** through Hub if offered. **Windows Build Support (IL2CPP)** is a separate optional module and is not required for this project's Mono build. This is not UWP.
3. Extract the ZIP to a writable folder on your Windows PC. In Hub choose **Add project from disk**, and select the `space-shooter-unity` folder containing `Assets`, `Packages`, and `ProjectSettings` — not `Assets` alone.
4. Open with **2022.3.62f3**. Allow package resolution, script compilation, and asset import to finish. Unity may need internet for Editor/license/package setup; the finished game makes no network requests.
5. `Assets/Editor/ProjectSetup.cs` automatically creates the scene and prefabs on first open. Wait for **“Orbit Breaker setup complete”** in the Console. The game scene opens automatically. Press **Play**, click the Game view, then **Enter** or **Launch**.
6. On later opens use **Orbit Breaker → Open Game Scene** if a different scene is open. If setup reports an error, read the first Console error rather than manually assembling objects. **Orbit Breaker → Rebuild Generated Assets** reruns setup and asks before overwriting generated content.

The project starts with intentionally **no hand-authored `.unity` or `.prefab` files**. The Editor creates native assets with correct Unity GUID references:

- `Assets/Generated/Scenes/OrbitBreaker.unity`: camera, audio listener, controller, wave director, audio source, parallax component, UI controller, and EventSystem.
- `Assets/Generated/Prefabs/`: Player; Enemy-Aimed, Enemy-Fan, Enemy-Radial; PlayerBullet, EnemyBullet; Powerup-Repair, Powerup-Rapid, Powerup-Triple.
- `Assets/Generated/GameAssets.asset`: all prefab, sprite, and audio references.
- Matching generated `.meta` files and the enabled build-scene entry.

UI widgets are fully created by `GameUI.Awake()` in Play mode; star-layer tiles by `ParallaxBackground.Start()`. Their construction code is part of the delivered game, not missing scene assembly. Unity also writes normal default settings and `Packages/packages-lock.json` during import. Included source assets and folders already carry stable `.meta` GUIDs.

### Controls and rules

| Input | Action |
|---|---|
| WASD or arrow keys | Move; diagonal speed is normalized |
| Hold Space | Fire |
| Enter | Launch / retry after game over |
| Escape or P | Pause / resume |
| R | Restart while paused or after game over |
| M | Toggle sound |
| Mouse | Click menu buttons |

Losing window focus pauses active play. Resume explicitly after returning.

Survive as many waves as possible. Shoot enemies for **100 / 160 / 220 points**. There is no final victory screen: this is an endless high-score game. You start with **5 hull points**. Enemy contact, enemy bullets, and escaped enemies each attempt 1 damage; **1.2 seconds of invulnerability** prevents stacked hits. Reaching zero hull ends the run.

Every third kill drops a pickup, cycling through:
- Green plus: restore 1 hull, capped at 5.
- Gold lightning: faster fire for 10 seconds.
- Purple arrows: triple shot for 12 seconds.

Picking up the same timed effect refreshes its duration. Both weapon effects can run together. Pausing freezes their timers. High score and sound preference persist locally through Unity PlayerPrefs.

Wave 1 uses aimed fire, wave 2 adds three-shot fans, and wave 3 introduces rotating eight-way bursts. Waves grow to a capped enemy count; fire rate, health, and speed increases also have caps. A wave finishes when all its spawned enemies are destroyed or leave the screen. Each restart uses the same seeded spawn sequence.

### Build for Windows x86_64

**On your Windows PC, in Unity:**

1. Exit Play mode. Run **Orbit Breaker → Validate Generated Project**. This checks serialized assets, actor bodies/colliders/layers, and the build-scene entry in Unity.
2. Choose **Orbit Breaker → Build Windows x64**. The default output is `Builds/Windows/OrbitBreaker.exe` inside your local project. The menu uses `BuildTarget.StandaloneWindows64` with Mono.
3. Run the executable on Windows. Distribute the **entire `Builds/Windows` folder**, including `OrbitBreaker_Data`, UnityPlayer DLLs, Mono files, and any other generated files — never just the `.exe`.

Manual alternative: **File → Build Settings → PC, Mac & Linux Standalone → Target Platform: Windows → Architecture: x86_64**. Switch Platform if necessary, ensure only the generated OrbitBreaker scene is enabled, then Build. In Player Settings retain **Mono**, **.NET Standard 2.1**, and **Input Manager (Old)**. Default presentation is a 1280×720 window with a fixed 16:9 game camera.

If you intentionally change to IL2CPP, install **Windows Build Support (IL2CPP)** plus the supported Visual Studio C++ toolchain and Windows SDK; that configuration is outside this project's default build path.

Optional batch commands **on your Windows PC**, after adjusting the example install/project paths:

```powershell
# Generate and validate without a build (requires a working, licensed Editor).
& 'C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe' `
  -batchmode -quit -projectPath 'C:\Projects\space-shooter-unity' `
  -executeMethod OrbitBreaker.Editor.ProjectSetup.SetupBatch -logFile 'C:\Projects\setup.log'

# Generate if needed, validate, then build Windows x64.
& 'C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe' `
  -batchmode -quit -projectPath 'C:\Projects\space-shooter-unity' `
  -buildTarget Win64 -executeMethod OrbitBreaker.Editor.ProjectSetup.BuildWindows `
  -logFile 'C:\Projects\build.log'
```

### Source map and collision setup

| File | Responsibility |
|---|---|
| `GameController.cs` | Title/play/pause/game-over states, score, audio, run cleanup, collision matrix |
| `PlayerController.cs` | Keyboard input, bounded movement, firing, health, invulnerability, upgrades |
| `Enemy.cs`, `WaveDirector.cs` | Seeded spawns, waves, movement, aimed/fan/radial shot patterns |
| `Bullet.cs`, `Powerup.cs` | Trigger ownership, hit consumption, lifetime, pickups |
| `GameUI.cs` | Complete uGUI title, HUD, pause, game-over and restart controls |
| `ParallaxBackground.cs`, `BurstEffect.cs` | Two scrolling star layers and short explosion effects |
| `FixedAspectCamera.cs`, `GameAssets.cs` | Camera framing and serialized asset catalogue |
| `Editor/ProjectSetup.cs` | First-open generation, validation and desktop build method |

Gameplay uses `Input.GetKey(KeyCode)` / `GetKeyDown`, not Input Actions. `activeInputHandler: 0` selects **Input Manager (Old)**. The four included named axes are for uGUI's `StandaloneInputModule`; gameplay movement does not depend on them. The newer Input System package is not installed.

Every actor prefab gets a **Dynamic Rigidbody2D**, zero gravity/drag, frozen rotation, interpolation, continuous collision detection and a **trigger Collider2D**. Movement occurs in `FixedUpdate`; the configured timestep is 1/60 second. The player hit circle is deliberately smaller than its artwork, and pickups are forgiving. These are 2D physics components, not 3D colliders.

| Layer | Index | Enabled game-layer contacts |
|---|---:|---|
| Player | 8 | Enemy, EnemyBullet, Powerup |
| Enemy | 9 | Player, PlayerBullet |
| PlayerBullet | 10 | Enemy |
| EnemyBullet | 11 | Player |
| Powerup | 12 | Player |

All other pairings among these five game layers are disabled. Setup applies the matrix and runtime reapplies it. Bullet owns projectile damage; Player owns ship contact; Powerup owns collection. Consume/death flags prevent duplicate score or pickups from overlapping trigger callbacks. Restart disables the old world before deferred destruction, clears its entities, and cancels the old wave coroutine.

### Validation and asset provenance

- `Validation.json`: measured offline check results and explicit test limits.
- `TESTING.md`: first-import / Play-mode / Windows acceptance checklist still to execute.
- `ASSETS.md`: original sprite, procedural sound and font provenance.
- `Tools/make_assets.py`: reproducible crops, geometric icons/effects, star textures and PCM WAV generation. Uses the included source atlas; no image-service account needed.
- `Tools/validate_source.py`: parse C# with Tree-sitter, verify metadata, PNG/WAV integrity and project configuration. **This is not a C# compiler or a Unity substitute.**

Python is **not needed** to open or build the game. Optional developer checks: `python -m pip install -r Tools/requirements.txt`, then `python Tools/validate_source.py`. Regenerate shipped assets with `python Tools/make_assets.py` only when you intend to overwrite their source import metadata.

### Version/API references

- [Unity 2022.3.62f3 official release](https://unity.com/releases/editor/whats-new/2022.3.62f3): pinned patch, not a claim that this is the newest Unity version.
- [Rigidbody2D.velocity](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Rigidbody2D-velocity.html): the 2022.3 API; deliberately not Unity 6's `linearVelocity`.
- [OnTriggerEnter2D](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.OnTriggerEnter2D.html): at least one involved collider needs a Rigidbody2D.
- [Input.GetKey](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Input.GetKey.html), [UGUI](https://docs.unity3d.com/2022.3/Documentation/Manual/com.unity.ugui.html).
- [API compatibility enum](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/ApiCompatibilityLevel.html): `NET_Standard`, not the deprecated `NET_Standard_2_0` enum name.
- [Windows64 target](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/BuildTarget.StandaloneWindows64.html).
