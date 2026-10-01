### Nova Guard — Unity / C# Windows desktop space shooter

A small, self-contained **2D desktop game**, not a web app. Fly a cyan ship, shoot incoming enemies, collect power-ups, and survive increasingly difficult waves. The mode is endless: the run ends when your hull reaches zero, and your goal is a new best score.

**Delivery status:** Full C# source, original bundled sprite/WAV assets, project settings, and an Editor scene/prefab generator are included. **Unity compilation, first-import generation, Play Mode, Windows building, and Windows gameplay have NOT been executed on the authoring Linux machine.** It has no Unity Editor or C# compiler installed. The supplied Python checks validate syntax trees, source contracts, configuration, and asset integrity only. No Windows EXE is included and no runtime screenshot is claimed.

### 1. Required software on your Windows development PC

- Windows 10/11, x86-64, with a Unity-compatible graphics driver.
- Unity Hub and a valid Unity license/sign-in as required by Unity.
- **Unity 2022.3.62f3 LTS**, the pinned patch. A newer security-patched 2022.3 LTS editor may be used, but has not been tested with this project. Do not downgrade to an older patch just to match a tutorial.
- Windows Standalone build support for that Editor. The Windows Editor normally includes the Mono desktop target; if Hub offers the required Windows support separately, install it through **Installs → gear → Add modules**. The build script checks target support and reports if it is unavailable.
- **Mono** scripting backend and **.NET Standard 2.1** API compatibility are set by the generator. IL2CPP and a Visual Studio C++ toolchain are not needed for this Mono build. Do not switch to IL2CPP unless you also install its Unity module and required Visual Studio/Windows SDK components.
- Optional C# editor: Visual Studio, Rider, or VS Code. No IDE is required to press Play or build from Unity.

The project uses Unity's built-in renderer, SpriteRenderer, Physics2D, legacy keyboard Input Manager, and uGUI. Unity/Hub may need internet access for editor installation and initial official-package resolution. **All game art and sounds are already bundled.** Python and the Python packages are only needed to rerun source checks or regenerate the supplied assets, never to open or build the game.

### 2. Open and generate the project

1. Extract the source ZIP to a writable folder on your Windows PC, for example `C:\Projects\space-shooter-unity`.
2. In Unity Hub, choose **Add / Add project from disk** and select the folder containing `Assets`, `Packages`, and `ProjectSettings`. Do not create a different Unity project or import only the Scripts folder.
3. Open with Unity 2022.3.62f3. Let package import and C# compilation finish.
4. `Assets/Editor/ProjectSetup.cs` schedules setup after import. On a clean first open it configures the project, imports the nine sprites, generates nine real `.prefab` assets, creates the real `.unity` scene, wires references, sets the build scene, and runs the Editor asset-wiring audit.
5. The generated scene is **`Assets/Generated/Scenes/NovaGuard.unity`**. It should open automatically. If it does not, use **Tools → Nova Guard → Generate Project**, then **Open Game Scene**. Setup will not discard an already-dirty scene: save it first.
6. Wait for the Console message describing generation, then press **Play** and click **Start Flight**.

No manual dragging of prefabs, sprites, sounds, layers, or button events is required. The cameras, EventSystem, world container, and game systems are saved into the generated scene. The menu/HUD hierarchy is constructed by `GameUI.cs` when the scene starts, so the UI is visible in Play Mode rather than as static edit-time Canvas children.

The generator is intentionally guarded: reopening an existing project does not overwrite the generated scene. **Regenerate Project replaces generated prefabs and the generated scene after a confirmation.** Keep custom scene/prefab edits outside `Assets/Generated` or back them up first. Batch-mode generation is explicit; it does not run from the automatic import callback.

### 3. Controls

| Action | Keyboard / mouse |
|---|---|
| Move | WASD or arrow keys; diagonals are normalized |
| Fire | Hold Space or Z |
| Pause / resume | Escape or P; HUD Pause button |
| Mute / unmute | M, or the HUD sound button |
| Restart after game over | R or Fly Again |
| Navigate menus | Arrow keys, Tab / Shift+Tab; Enter or Space to activate |
| Mouse menus | Click buttons |
| Quit | Quit to Desktop on the main, pause, or game-over screen |

Switching away from the game pauses an active run. Refocusing does not auto-resume: press Escape/P or Resume. The window is resizable; the camera letterboxes to a consistent 16:9 playfield.

### 4. Gameplay rules

- **Hull:** 5 points. Hostile bullets, enemy contact, or an escaped enemy cost 1 hull point. Damage grants 1.25 seconds of invulnerability with a blink. Initial spawn protection lasts 1.5 seconds.
- **Enemy scout:** red ship, straight downward fire, faster movement; base health 2, 100 points.
- **Enemy gunner:** magenta ship, aimed fire; appears from wave 2; base health 3, 175 points.
- **Enemy tank:** orange ship, five-way aimed spread; appears from wave 3; base health 7, 350 points.
- **Waves:** increase enemy count, movement speed, fire frequency, and eventually health, with upper caps. A wave clears only after all its spawned enemies are destroyed or escape. Clearing awards `wave × 100` points and a short breather.
- **Repair:** green plus, restores up to 2 hull without exceeding 5.
- **Triple shot:** gold lightning, faster three-bolt fire for 10 seconds.
- **Shield:** blue shield, prevents damage for 8 seconds and tints the ship blue.
- Destroyed enemies have a 22% pickup chance. One additional pickup is guaranteed at every wave clear, cycling repair → triple shot → shield. Collecting any pickup adds 25 points. Recollecting a timed buff refreshes its duration rather than stacking forever.
- Best score and mute preference persist through Unity PlayerPrefs. On Windows the default player key is under `HKCU\Software\Nova Guard Studio\Nova Guard`. There are no accounts, network calls, or online leaderboards.
- Three differently paced star layers create scrolling parallax; sparks accompany damage, explosions, and pickups. Eight original WAV effects cover firing, hits, explosions, pickups, waves, game over, and UI clicks.

### 5. Compile a Windows x86-64 executable

#### Recommended: Unity menu

On your **Windows development PC**:

1. Open the generated game scene and resolve any Console compilation errors before building.
2. Run **Tools → Nova Guard → Validate Generated Assets**. This checks imported prefabs, sprites, colliders, audio, build-scene registration, and scene references; it is not a gameplay test.
3. Choose **Tools → Nova Guard → Build Windows x64**.
4. The script builds `Builds\Windows\NovaGuard.exe` using `BuildTarget.StandaloneWindows64` and reports success or throws an error with a Console/log pointer.
5. Run the executable on Windows and complete the checklist in `docs/WINDOWS_TEST_CHECKLIST.md` before distributing it.

#### Alternative: standard Build Settings

Open **File → Build Settings → PC, Mac & Linux Standalone**. Set **Target Platform: Windows**, **Architecture: x86_64**, and switch platform if requested. Ensure only the enabled generated `NovaGuard` scene is in Scenes In Build. Under Player Settings use **Mono** and **.NET Standard 2.1**. Click Build and choose `Builds\Windows\NovaGuard.exe`.

#### Optional: command line on Windows

Close the project's Unity Editor first so the batch process can acquire the project lock. From the extracted project folder run:

```bat
BuildWindows.cmd
```

For a custom editor installation:

```bat
BuildWindows.cmd "D:\Unity\2022.3.62f3\Editor\Unity.exe"
```

The helper writes `build-windows.log`. Direct equivalent, executed **on your Windows PC**, not on this Linux machine:

```bat
"C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Projects\space-shooter-unity" -executeMethod ProjectSetup.BuildWindows -logFile "C:\Projects\space-shooter-unity\build-windows.log"
```

For an explicit first-import regeneration in automation, the public entry point is `-executeMethod ProjectSetup.GenerateBatch`. It overwrites generated assets without the interactive confirmation. Never run it over custom changes you have not saved elsewhere.

#### Distributing the game

Send the **entire contents of `Builds\Windows`**, preferably zipped, not just `NovaGuard.exe`. Keep `NovaGuard_Data`, `UnityPlayer.dll`, `MonoBleedingEdge`, and any crash handler or other files Unity emits together with the EXE. Recipients extract the complete folder and launch `NovaGuard.exe`; they do not need Unity or this source project. This project does not create an installer or code-sign the player; Windows SmartScreen may warn about an unsigned build. Do not tell recipients to disable their security protections.

### 6. Source map

```text
Assets/
  Art/                  Original sprite sheet + nine cropped RGBA game sprites
  Audio/                Eight original 44.1 kHz mono PCM WAV files
  Editor/
    ProjectSetup.cs     First-import generation and Windows build command
    ProjectValidation.cs  Unity-side generated-asset audit
  Scripts/
    GameController.cs   State machine, run cleanup, spawning, scoring, persistence
    KeyboardInput.cs    Built-in keyboard input abstraction
    PlayerShip.cs       Movement, firing, health, invulnerability, buffs
    EnemyShip.cs        Three enemy behaviors and bullet patterns
    Bullet.cs           Projectile motion, lifetime and damage routing
    PowerUp.cs          Pickup motion and collection
    WaveDirector.cs     Wave spawning, progression and clear rewards
    PhysicsRules.cs     Collision-layer matrix and fixed-step configuration
    Starfield.cs        Layered scrolling starfield
    HitBurst.cs         Short-lived spark feedback and resource cleanup
    SoundBank.cs        WAV playback, enemy-audio throttling and mute persistence
    GameUI.cs           Menus, HUD, keyboard/mouse navigation and button callbacks
    FixedAspect.cs      Consistent playfield and letterboxing on window resize
  Generated/            CREATED BY UNITY: scene and nine prefabs
Packages/manifest.json
ProjectSettings/        Version, input, layers, editor and desktop settings
Tools/                  Optional Python asset preparation and static validation
BuildWindows.cmd        Optional Windows batch build helper
```

To tune the game, change the exposed fields in generated prefabs or the generator defaults. Core boundaries and score rules live in `GameController`; wave cadence in `WaveDirector`; drop chance in `GameController.DropPowerUp`. Changes to scripts are not overwritten by generation.

### 7. Validation and troubleshooting

- `docs/SOURCE_CHECKS.json` records the automated source/config/asset check result. These checks parse C# but **do not resolve Unity APIs, compile an assembly, simulate physics, or prove UI/gameplay correctness**.
- `docs/VALIDATION.md` explains the exact verification boundary and remaining import/runtime risks.
- `docs/WINDOWS_TEST_CHECKLIST.md` is an unexecuted acceptance checklist for your Unity/Windows machine.
- To repeat source checks with Python: `python -m pip install -r Tools/requirements.txt`, then `python Tools/validate_source.py`.
- If the project opens with no scene, first read the Unity Console. When compilation succeeds, use **Tools → Nova Guard → Generate Project** and **Open Game Scene**.
- If input is disabled, ensure **Project Settings → Player → Other Settings → Active Input Handling** is **Input Manager (Old)** or **Both**, not new Input System only. This project deliberately uses Unity's built-in keyboard system.
- If a package cannot resolve, let Unity finish its official package restore and check connectivity. No Asset Store package or downloaded art pack is required.
- If a Windows build target is unavailable, install that editor's matching Windows support through Unity Hub. If your editor is using IL2CPP, restore Mono or install the separate IL2CPP toolchain.
- If compilation or generation fails, retain the **first full Console error**, its script and line number, and the exact Editor version. Do not treat the static-check report as evidence that the Unity build succeeded.

### 8. Asset provenance and API references

See `ASSETS.md` for the generated-art and original-audio provenance. Unity and its official packages retain their own licenses.

Official references used while authoring:

- [Pinned Unity release](https://unity.com/releases/editor/whats-new/2022.3.62f3)
- [EditorApplication.delayCall](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/EditorApplication-delayCall.html)
- [PrefabUtility.SaveAsPrefabAsset](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PrefabUtility.SaveAsPrefabAsset.html)
- [BuildPipeline.BuildPlayer](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html)
- [Windows 64-bit target](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/BuildTarget.StandaloneWindows64.html)
- [API compatibility levels](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/ApiCompatibilityLevel.html)
