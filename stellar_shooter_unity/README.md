### Stellar Shooter — Unity / Windows desktop

A small, complete-source **2D Windows desktop space shooter in C#**. This is not a web app. Survive endless waves, collect upgrades, and improve your local high score.

**Pinned editor:** Unity **6000.0.62f1**, Unity 6.0 LTS, revision `f99f05b3e950`. Built-in rendering pipeline, Unity UI (`com.unity.ugui` **2.0.0**), Mono scripting backend, Windows **Intel/AMD x86_64** target. This is a reproducible version pin, not a claim to be the latest editor.

**Important delivery boundary:** PNG sprites, WAV effects, C# source, project settings, and the complete Editor generator are included. The serialized scene and prefabs are created **inside Unity by one menu command**; they are not pre-generated in this ZIP. No manual Inspector wiring, sprite slicing, scene assembly, or asset downloads are needed. Unity Editor is unavailable in the authoring environment, so Unity compilation, generation, Play mode, and a Windows executable build/runtime have **not been tested**. No prebuilt `.exe` is included.

### 1. Install and open on your Windows PC

1. Install [Unity Hub](https://unity.com/download), sign in, and activate an appropriate Unity license. A Unity Personal license is sufficient if you meet Unity's eligibility requirements.
2. Open the [6000.0.62f1 release page](https://unity.com/releases/editor/whats-new/6000.0.62f1) and choose its **Install in Unity Hub** link. Alternatively use the [Editor download archive](https://unity.com/releases/editor/archive). Select the exact version above.
3. Install the **Windows Editor**. Windows Mono player support is normally included in that installation. If Hub offers a separate Windows target-support module, include it. **Windows Build Support (IL2CPP)** and Visual Studio's C++ tools are optional and are not used by this project's Mono build helper. A C# IDE is optional for editing source.
4. Extract the ZIP, for example to `C:\UnityProjects\stellar_shooter_unity`. Choose the folder that directly contains **Assets**, **Packages**, and **ProjectSettings**, not the ZIP's outer parent.
5. In Hub, choose **Projects → Add → Add project from disk**, select that folder, and open it with **6000.0.62f1**. Allow package resolution and script compilation to finish. Unity writes the remaining standard settings, its package lock file, and local caches on import.
6. In the Editor menu, choose **Stellar Shooter → 1 - Generate Project**. Wait for the Console message ending in **“Press Play.”** The generated scene opens automatically.
7. Press the Editor's **Play** button. Click the Game tab to focus it, then select **START FLIGHT** with Enter or the mouse.

If the Stellar Shooter menu is missing, open **Window → General → Console** and inspect the first red compilation/package error; verify the pinned editor version and allow package import to finish. Do not create a different template project or add URP/the new Input System.

### 2. Controls and rules

| Action | Key |
|---|---|
| Move | WASD or arrow keys; diagonal movement is normalized |
| Fire continuously | Hold Space |
| Pause / resume | Escape or P |
| Select menu item | Arrow keys (or W/S), then Enter; mouse clicks also work |
| Mute / unmute | M or the on-screen sound button |
| Restart after game over | R or FLY AGAIN |
| Restart / main menu / quit | Pause-menu buttons; Quit also exists on title/game-over screens |

The game starts with **5 hull**. A hostile shot, enemy collision, or escaped enemy costs one hull unless protected. Taking damage grants **1.15 seconds** of invulnerability. Losing all hull ends the flight; there is no final victory wave in this endless score-attack game. Losing window focus pauses an active flight. Quit stops Play mode in the Editor and exits the executable in Windows.

- **Scout:** aimed single shots; available from wave 1.
- **Gunner:** downward three-shot fan; available from wave 2.
- **Bomber:** eight-direction radial shots; available from wave 3.
- Wave size, enemy hull, movement speed, and firing rate increase with bounded difficulty. A wave ends when all its spawned enemies have been destroyed or have left the screen.
- Destroyed scouts/gunners/bombers award **100 / 150 / 200** points. Ramming enemies does not award points.
- Every fourth scored kill drops a pickup, cycling **Spread → Repair → Shield**. Spread grants triple shots for **12 seconds**; Repair restores up to **2 hull**; Shield protects for **8 seconds**. Recollecting a timed upgrade refreshes its duration rather than stacking it.
- Best score is stored locally with Unity `PlayerPrefs` when a flight ends. Sound preference is saved immediately. Restart resets hull, score, enemies, projectiles, power-ups, and wave progression.
- Two scrolling generated-art layers provide parallax. Eight original WAV cues cover firing, impact, destruction, pickups, waves, game-over, and menu actions.

**Input choice:** gameplay polls Unity's built-in legacy `UnityEngine.Input.GetKey/GetKeyDown` using explicit keys. This intentionally small fixed-key desktop game does **not** depend on the newer Input System package or `.inputactions` assets. `activeInputHandler: 0` selects **Input Manager (Old)**. The supplied InputManager axes are for `StandaloneInputModule` menu navigation, not for player movement. The Editor generator preserves this setting; if you change it later, use **Edit → Project Settings → Player → Other Settings → Active Input Handling → Input Manager (Old)** and restart the Editor if prompted. Unity recommends its newer Input System for new general-purpose projects; the older API is intentionally used here to minimize setup.

### 3. Build the Windows executable

On your **Windows PC**, exit Play mode and choose:

**Stellar Shooter → 3 - Build Windows x64**

The helper generates the scene if missing, checks references, selects a desktop Player target (not a server), explicitly sets **x64**, selects **Mono**, and calls `BuildPipeline.BuildPlayer`. A failed build throws an error instead of reporting success.

Output, relative to the project folder:

```text
Builds/Windows/
  StellarShooter.exe
  StellarShooter_Data/
  UnityPlayer.dll
  MonoBleedingEdge/
  ...other Unity-generated support files...
```

**Distribute the entire `Builds/Windows` folder together.** Do not move or send the `.exe` by itself. Keep the `_Data` folder and all generated libraries next to it. End users do not need Unity Hub or the Unity Editor to play. Use a compatible Windows 10/11 64-bit desktop with functioning graphics/audio drivers. The executable is unsigned; Windows may show an unknown-publisher warning. Distribute through a trusted channel and code-sign releases if appropriate; do not disable Windows security globally.

The underlying manual alternative is **File → Build Profiles → Windows**, desktop Player, **Intel 64-bit**, Mono in Player settings, with `Assets/StellarShooter/Generated/Scenes/StellarShooter.unity` included. The provided menu is simpler and avoids accidentally choosing ARM64 or server builds.

#### Optional command-line build on your Windows PC

Close any Editor instance using this project first. In **PowerShell**, after adjusting `$project` to your extracted folder:

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.0.62f1\Editor\Unity.exe'
$project = 'C:\UnityProjects\stellar_shooter_unity'
& $unity -batchmode -quit -projectPath $project -buildTarget Win64 `
  -executeMethod StellarShooter.Editor.ProjectBuilder.Generate `
  -logFile "$project\Generate.log"
if ($LASTEXITCODE -ne 0) { throw 'Generation failed; inspect Generate.log' }
& $unity -batchmode -quit -projectPath $project -buildTarget Win64 `
  -executeMethod StellarShooter.Editor.ProjectBuilder.BuildWindows `
  -logFile "$project\BuildWindows.log"
if ($LASTEXITCODE -ne 0) { throw 'Build failed; inspect BuildWindows.log' }
```

A licensed, activated Editor and Windows target support are still required in batch mode. The command does not bypass licensing. The generation command intentionally replaces generated scene/prefab edits.

### 4. Project structure and customization

```text
Assets/StellarShooter/
  Scripts/
    GameRules.cs           Pure balance values and bullet-pattern math
    GameSession.cs         Menu/play/pause/game-over lifecycle, score, drops
    PlayerController.cs    Keyboard movement, firing, health and timed upgrades
    Enemy.cs               Movement, enemy patterns, collisions and death
    Projectile.cs          Shot motion, ownership, collision and expiry
    Pickup.cs              Upgrade collection and drift
    WaveDirector.cs        Repeatable spawn sequence and wave progression
    SoundBank.cs           WAV playback, throttling and saved mute preference
    GameUI.cs              Menu actions, HUD, scores and focus selection
    ParallaxLayer.cs       Two-tile scrolling layer
    ImpactEffect.cs        Short sprite-burst feedback
    FixedAspectCamera.cs   Stable 16:9 arena on resized windows
  Editor/ProjectBuilder.cs Import/setup, prefab/scene serialization, validation/build
  Art/                    Nine original sprite PNGs and two background PNGs
  Audio/                  Eight original PCM WAV effects
  Generated/              Created by the Editor command, not shipped pre-generated
    Prefabs/              Player, two shots, three enemies, three pickups, impact
    Scenes/StellarShooter.unity
Packages/manifest.json
ProjectSettings/          Version, player/input/layer settings
SourceArt/sprite-sheet.png
Tools/                    Optional asset preparation, metadata and offline checks
ASSETS.md                 Artwork/audio provenance
```

**Regeneration:** the generator reuses stable prefab paths and replaces the generated scene and prefabs. It warns before overwriting when run from the menu. It does not erase your source scripts, original art, or WAVs. Save custom generated-asset edits elsewhere before regenerating. The build command uses an existing generated scene without regenerating it. All runtime object references, colliders, Rigidbody2D settings, sprites, sorting orders, UI controls, EventSystem, audio clips, and build scene entries are assigned by code.

Change balance in `GameRules.cs`; spawn cadence in `WaveDirector.cs`; player firing/movement in `PlayerController.cs`; colors/layout in `ProjectBuilder.cs`. Sprite units use 256 pixels per Unity unit. Layers 8–12 are reserved for this project's collision categories. Rigidbody2D trigger pairs are explicitly restricted at runtime; bullets use continuous detection and finite lifetimes. The project uses simple instantiate/destroy rather than a complex object-pool framework.

The Python tools are **not required** to open, generate, play, or build. To reproduce the bundled image crops/audio, install Pillow and run `python Tools/prepare_assets.py`; its input artwork is already on disk. `write_metadata.py` creates stable GUIDs only for assets that lack metadata. Keep `.meta` files when copying or versioning Unity assets.

### 5. Verification status and checks to run in Unity

Completed in the Linux authoring environment:

- **222** offline assertions: C# syntax parsing, pinned settings, source/asset paths, GUID uniqueness, asset completeness and format, sprite transparency/margins, WAV decoding/non-silence/clipping, and setup/build operation presence.
- **14 C# files** parsed: the 13 Unity source files plus the standalone rules test.
- Actual C# compilation of `GameRules.cs` with `Tools/RulesTests.cs` using Mono, followed by **82 passing rule assertions**. These cover wave caps, health, repair, scoring, difficulty, bullet counts/angles, and a fixed-step displacement bound.
- Original sprite crops and background visually inspected. These are asset inspections, **not screenshots of Unity gameplay**.

**Not performed:** Unity package resolution/import, Unity assembly/API compilation, scene/prefab generation, Unity Play mode, Windows build, or Windows runtime testing. Syntax parsing and pure-rule tests cannot prove those paths work. Remaining risks include Editor-specific API/package resolution, actual serialized scene import, UI layout/focus at runtime, and physics/audio behavior; the first Unity run must check them.

On your Windows PC, after generation:

1. Run **Stellar Shooter → 2 - Validate Generated Project**. This opens the generated scene and checks assigned scene/UI/audio/prefab references, sprites, missing scripts, and its build-list entry. It is a setup check, not a gameplay test.
2. Enter Play mode. Check title-menu keyboard and mouse selection; move in all directions, verify arena bounds, and hold Space. Destroy an enemy and confirm score increases.
3. Reach waves 2 and 3 to check the fan and radial patterns. Kill four enemies to collect Spread, then eight for Repair, then twelve for Shield. Verify HUD timers and effects.
4. Take a hit; check hull loss and temporary blinking. Let an enemy escape to check hull loss. Reach zero hull; check game-over results, R restart, and a clean fresh wave.
5. Pause while shots/enemies are moving. Confirm movement, spawning and upgrade timers freeze; resume without jumps. Restart from pause twice, then return to main menu.
6. Toggle mute, lose focus, resize the window, and test Quit. Build Windows x64, run the `.exe` from its full output folder, and repeat the key checks. Reopen the executable to check saved best score and mute preference.

To rerun the offline checks on a machine with Python, Pillow, `tree-sitter`, `tree-sitter-c-sharp`, and Mono installed:

```text
python Tools/validate_source.py
mcs -warnaserror -out:RulesTests.exe Assets/StellarShooter/Scripts/GameRules.cs Tools/RulesTests.cs
mono RulesTests.exe
```

### References used for version/API choices

- [Pinned release and revision](https://unity.com/releases/editor/whats-new/6000.0.62f1)
- [Unity 6 UI package documentation](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.ugui.html)
- [Unity-maintained project using UGUI 2.0.0](https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop/blob/main/Packages/manifest.json)
- [Legacy keyboard API](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Input.GetKey.html)
- [Unity 6 Rigidbody2D.linearVelocity](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Rigidbody2D-linearVelocity.html)
- [Unity 6 build scripting](https://docs.unity3d.com/6/Documentation/Manual/build-script-build.html)
- [Windows output binaries](https://docs.unity3d.com/6000.0/Documentation/Manual/WindowsStandaloneBinaries.html)
- [Unity 6000.0 reference source for Windows architecture settings](https://github.com/Unity-Technologies/UnityCsReference/blob/6000.0/Editor/Mono/EditorUserBuildSettings.bindings.cs)
