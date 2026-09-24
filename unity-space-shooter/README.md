### STARWARD — Unity 2D Windows Desktop Space Shooter

A small, fully source-delivered C# arcade survival game. This is a **Windows desktop Unity project**, not a website or WebGL app. No server, account, API key, Asset Store package, external service, or network connection is needed by the game.

**Verification boundary:** source and media checks were performed on a Linux VM without the Unity Editor. Actual Unity import/compilation, scene generation, Play-mode behavior, and a Windows executable build have **not** been verified. No Windows game executable is included. See `Docs/Validation.json` for the machine-readable checks and `Docs/WindowsTestChecklist.md` for the remaining checks.

### 1. Exact editor and dependencies

- **Unity 6.3 LTS, editor 6000.3.11f1**, pinned in `ProjectSettings/ProjectVersion.txt`.
- Official editor release: https://unity.com/releases/editor/whats-new/6000.3.11f1
- The Unity 6.3 LTS line is supported through December 2027: https://unity.com/releases/unity-6/support
- Built-in rendering pipeline, orthographic 2D camera, built-in SpriteRenderer and Physics2D.
- **Input Manager (Old)**, explicitly `activeInputHandler: 0`. All controls use `Input.GetKey`/`GetKeyDown`; custom axes are unnecessary.
- Only four pinned built-in modules, each `1.0.0`: audio, imgui, physics2d, imageconversion. No URP, new Input System, TextMeshPro, uGUI, or third-party runtime packages.
- Desktop UI uses Unity IMGUI deliberately to keep setup small and inspectable.
- Windows x86_64, Mono scripting backend. IL2CPP and its Visual C++ toolchain are not required by this project.
- The pinned patch is a reproducible target, not a claim that it is the latest patch. Opening a newer 6000.3 LTS patch may rewrite settings; back up first.

### 2. Import through Unity Hub on your Windows computer

1. Download and extract `unity-space-shooter.zip` to a writable local folder. Do not open the project from inside the ZIP.
2. Install Unity Hub and sign in/activate a suitable Unity license if Hub requires it.
3. Install editor **6000.3.11f1**, using its release page's Unity Hub link if Hub does not list that patch. Windows Mono standalone support is normally included with the Windows editor. If Unity reports Windows support missing, add that editor's Windows build support through **Hub > Installs > gear/menu > Add modules**. You do not need WebGL support.
4. In **Hub > Projects > Add > Add project from disk**, select the extracted `unity-space-shooter` folder — the one containing `Assets`, `Packages`, and `ProjectSettings`. Do not create a new Unity template and do not select `Assets` itself.
5. Open the project with the pinned editor. Allow package resolution, asset import, and script compilation to finish.
6. The first-open editor utility automatically generates the game. Wait for the Console message **“Starward setup complete”**.
7. If automatic setup was deferred, choose **Starward > Create or Rebuild Game** once. If the menu is missing, inspect the Unity Console for a compiler error first; the menu cannot load until compilation succeeds.
8. The generated `Assets/Generated/Scenes/Starward.unity` scene opens. If needed, double-click it in the Project window. Choose **Starward > Validate Generated Project** to check references.
9. Press Unity's **Play** button, click the **Game** tab so keyboard input has focus, then click **START RUN** or press **Enter**.

**Input warning:** if Unity offers to enable the new Input System, keep **Input Manager (Old)**. Confirm under **Edit > Project Settings > Player > Other Settings > Configuration > Active Input Handling**. If you change it, accept Unity's restart prompt.

### 3. Scene and prefab generation — included, not omitted

Unity scene/prefab YAML is intentionally not hand-authored. The full deterministic generator is `Assets/Editor/ProjectSetup.cs`. It creates actual `.unity` and `.prefab` assets in your Unity Editor, assigns every gameplay reference, configures sprite imports/layers, and registers the scene in build settings.

Generated assets:

```text
Assets/Generated/
  Scenes/Starward.unity
  Prefabs/Player.prefab
  Prefabs/Enemy.prefab
  Prefabs/PlayerBullet.prefab
  Prefabs/EnemyBullet.prefab
  Prefabs/RepairPickup.prefab
  Prefabs/WeaponPickup.prefab
  Prefabs/Burst.prefab
```

The scene contains a camera/listener, GameManager, WaveDirector, AudioBank, GameUI, runtime entity container, and layered Starfield. Setup runs automatically only while the generated scene is absent. It leaves an existing scene untouched on subsequent opens. A failed attempt is not retried in a loop; fix the Console error and run the menu command.

**Rebuild is idempotent:** it writes the same seven prefab paths and replaces the generated scene, rather than appending duplicates. Explicit rebuild requests ask before overwriting existing generated assets. Copy customized generated scenes/prefabs elsewhere before rebuilding. Runtime scripts and original art/audio are never replaced by this menu.

The source archive has no pre-generated Unity scene/prefabs because the editor was not run on the development VM. First-open generation is therefore a required step, not an optional example. You do not have to assemble a scene manually or drag inspector references yourself.

### 4. Controls and rules

| Control | Action |
|---|---|
| WASD or arrow keys | Move; diagonal movement is normalized |
| Hold Space | Fire continuously |
| Enter | Start from title / restart from game over |
| R | Restart from game over |
| Esc or P | Pause / resume |
| M or HUD sound button | Toggle sound; preference persists |
| Menu buttons | Start, resume, restart, main menu, quit |

- Survive as many waves as possible. This is endless survival, not a finite campaign with a win screen.
- HUD shows score, best, wave, enemies remaining, hull health, and weapon level.
- Start with **5 hull** and **2 seconds of spawn protection**. Taking damage grants **1.25 seconds of invulnerability**, indicated by flashing, and reduces the weapon level by one (minimum 1).
- Enemies crossing the bottom edge damage your hull unless invulnerability is active. Ramming an enemy also damages hull. Hull reaching zero ends the run.
- Enemy types alternate in a repeatable pattern: red ships fire straight; pink-tinted ships fire three-shot fans.
- Each kill gives `100 + 10 × wave` points. Waves add enemies, health, speed, and firing frequency, all capped for bounded difficulty parameters.
- Defeat all spawned enemies or survive their departure to advance; intermissions last approximately 2.5 seconds.
- Every fourth kill drops a pickup, alternating **gold weapon upgrade** then **green repair**. Repair gives +2 health up to 5. Weapons cap at level 3: one, two, then three shots per volley.
- Best score is saved using local Unity PlayerPrefs at game over, menu exit, or application quit. Mute preference also persists.
- Leaving the application window pauses the run. Resume explicitly when you return.
- The fixed 16:9 arena is letterboxed/pillarboxed in other window shapes; the UI uses the same scaling.

### 5. Build the Windows desktop executable

On your **Windows computer**, after setup and the manual Play checks:

1. Exit Play mode.
2. Open **File > Build Profiles** (Unity 6 terminology).
3. Add/select the **Windows** platform profile. Choose **Intel 64-bit / x86_64**, not ARM64. Click **Switch Platform** if Windows is not active and wait for recompilation.
4. Confirm **Player Settings > Other Settings > Scripting Backend = Mono**. Setup applies this setting automatically.
5. Choose **Starward > Build Windows x86_64**. The command validates generated references, explicitly builds the one game scene with `BuildTarget.StandaloneWindows64`, and reports failure if the build does not succeed.
6. Output appears under the extracted project's `Builds/Windows/Starward.exe`.
7. Launch that EXE on Windows and run the checks in `Docs/WindowsTestChecklist.md`.
8. Share the **entire `Builds/Windows` folder**, not the EXE alone. Keep `Starward_Data`, `UnityPlayer.dll`, Mono/runtime folders, and other Unity-generated files beside it.

Manual alternative: use the active Windows profile's scene list with `Assets/Generated/Scenes/Starward.unity` enabled, then Build to `Builds/Windows/Starward.exe`.

Optional batch build, in **PowerShell on your Windows computer** (replace the project/log paths with your extraction location):

```powershell
$Unity = "C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe"
$Project = "C:\Projects\unity-space-shooter"
& $Unity -batchmode -quit -projectPath $Project -buildTarget Win64 -executeMethod StarwardEditor.BuildGame.BuildWindows -logFile "$Project\windows-build.log"
```

An activated editor and Windows support must already be installed. The method generates the scene if missing. A successful source check on Linux is **not** a substitute for this Unity build.

### 6. Code map

| File under Assets | Responsibility |
|---|---|
| `Scripts/GameManager.cs` | State machine, run lifecycle, scoring, persistent best, drops, entity cleanup |
| `Scripts/GameRules.cs` | Pure progression, damage/heal and spread rules |
| `Scripts/PlayerController.cs` | Keyboard input, bounded motion, weapon firing, damage feedback |
| `Scripts/Health.cs` | Health, protection timer, exactly-once death notification |
| `Scripts/Enemy.cs` | Enemy movement, straight/fan patterns, ramming/escape/destruction |
| `Scripts/WaveDirector.cs` | Bounded progressive waves and deterministic spawn lanes |
| `Scripts/Bullet.cs` | Friendly/hostile ownership, swept collision checks, trigger fallback, lifetime/bounds cleanup |
| `Scripts/Pickup.cs` | Repair/weapon collection and cleanup |
| `Scripts/AudioBank.cs` | Eight local clips, bounded 12-voice playback, persisted mute |
| `Scripts/Starfield.cs` | Three independently scrolling star layers |
| `Scripts/Burst.cs` | Short-lived hit/explosion/pickup sparks |
| `Scripts/GameUI.cs` | Title, HUD, pause, game over and desktop menu actions |
| `Scripts/WorldCamera.cs` | Aspect-preserving fixed camera |
| `Editor/ProjectSetup.cs` | Automatic/menu scene and prefab generator |
| `Editor/BuildGame.cs` | Generated-asset validation and explicit Windows x86_64 build |

Layers 8–12 are Player, Enemy, PlayerBullet, EnemyBullet, Pickup. Actors are kinematic 2D rigidbodies with trigger colliders and full kinematic contacts. Bullets sweep a small circle through their next physics step using a target-only layer mask to avoid tunneling; triggers provide the normal contact path too. Consumed objects are disabled immediately before deferred destruction, preventing duplicate hits or collection. Physics and simulation pause together using `Time.timeScale`.

### 7. Assets and offline development checks

`ASSETS.md` records provenance. All runtime assets are local. `Docs/SpritePreview.png` is an asset contact sheet, **not a gameplay screenshot**.

Optional authoring tools, not required by Unity or players:

- `Tools/prepare_assets.py`: recrops the included original atlas and regenerates the seven synthesized WAV effects; requires Python, Pillow, numpy. The generated laser MP3 is already included.
- `Tools/write_project_metadata.py`: initial metadata/bootstrap settings writer. **Do not rerun after customizing Unity project settings**; it overwrites bootstrap settings. It preserves existing asset GUID files.
- `Tools/validate.py`: repeats Linux source/media checks with Python + Pillow, Mono (`mcs`/`mono`) and FFmpeg (`ffprobe`). Run `python3 Tools/validate.py` from the project folder.
- `Tools/UnitySourceCheckStubs.cs`: deliberately nonfunctional API signatures used only by offline C# checks; outside `Assets`, never included in Unity or the game.
- `Tools/SourceCheckTests.cs`: 51 assertions on rules and health methods against those stubs. They do not simulate Unity physics, lifecycle, input, UI, rendering, or audio.

Actual Unity/Windows checks remain listed as unverified in the validation report. If Unity produces an error, the relevant evidence is its complete Console error and stack trace, or the Windows batch build log — not the offline stub check.
