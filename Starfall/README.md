### STARFALL — native Unity desktop space shooter

A compact, single-player **2D Unity/C# Windows desktop game**, not a web app. No server, accounts, browser, paid assets, URP, or third-party gameplay packages. Uses Unity's built-in renderer, 2D physics, keyboard input, and IMGUI menus.

**Verification boundary:** this project was authored on Linux without Unity Editor. C# syntax and project/scene metadata were checked, and engine-independent rules were compiled and tested with Mono. **Unity API compilation, editor import/Play mode, generated assets, rendered visuals, audio, and Windows execution have NOT been verified. No executable is included.** See `VERIFICATION.md` for exact results and a PC smoke-test checklist.

### Open and play — on your Windows PC

1. Install **Unity Hub** and **Unity Editor 2022.3.70f1 LTS**. Add **Windows Build Support (Mono)** if Hub offers it as a separate module; some Windows Editor installations already include this target. A free Unity license is sufficient if you meet Unity's licensing eligibility.
   - Documented release: https://unity.com/releases/editor/whats-new/2022.3.70f1
   - This is a pinned LTS release, not a claim that it is the latest. Prefer a newer security-patched 2022.3 release when distributing publicly; re-test after upgrading. Do not downgrade to older vulnerable patches.
2. Download the project folder. In Hub choose **Projects → Add → Add project from disk**, then select the folder containing `Assets`, `Packages`, and `ProjectSettings`. Open with the specified Editor. Do not create a new URP project or copy only the scripts.
3. Wait for compilation/import. The editor automatically generates the original sprites, sound files, prefab assets, and catalog the first time it opens. Look for **“Starfall: content generation and serialized reference checks passed.”** in the Console. If it does not appear, use **Starfall → Rebuild Generated Content** outside Play mode.
4. Choose **Starfall → Open Game Scene**, or open `Assets/Starfall/Scenes/Starfall.unity`. Press the Editor's **Play** button, click the Game view to focus it, then press **Enter** to launch.
5. The scene is a deliberately small, committed **bootstrap scene**. Its `SceneEntry` creates the camera and game controller at runtime; the menu appears in Play mode, not in the edit-time Scene view. Ships, bullets, and pickups are instantiated from the generated prefab assets. No Inspector wiring is required.

If Unity reports that the project uses the new input backend, choose **Edit → Project Settings → Player → Other Settings → Active Input Handling → Input Manager (Old)** and restart the Editor when prompted. The supplied settings and builder already select this backend. Input uses `Input.GetKey`; no named axes are needed.

### Controls

| Action | Keyboard |
|---|---|
| Move | WASD or arrow keys |
| Shoot continuously | Hold Space or Z |
| Precision movement | Hold Left Shift |
| Start / retry | Enter; R also retries after game over |
| Pause / resume | Escape |
| Toggle sound | M |

Menus also have clickable buttons for launching, resuming, returning to the menu, sound, and quitting. Losing application focus pauses an active run. Sound preference and best score persist locally through Unity PlayerPrefs.

### The game

- Five hull points, collision damage, and a short flashing invulnerability window after taking damage.
- Aim shots from scouts, five-shot fans from armored enemies, and rotating eight-shot radial patterns from turrets.
- Endless waves: fans unlock in wave 2, radial turrets in wave 4. Spawn counts, movement speed, and health growth are capped.
- Shooting enemies awards points; ramming and enemies escaping offscreen do not.
- Every fifth shooting kill drops a power-up, cycling **spread → repair → rapid**:
  - **Violet trident:** three-way fire for 12 seconds.
  - **Green plus:** restore two hull points, capped at five.
  - **Gold bolt:** faster fire for 9 seconds.
- Rapid and spread can combine. Collecting the same timed power-up refreshes its duration, rather than stacking indefinitely.
- Three-depth scrolling stars, hit/explosion sparks, original geometric spacecraft sprites, and six synthesized sound effects.
- Start, pause, game-over/retry, score, hull, wave announcements, power-up timers, best score, and mute UI.
- A 1280×720 resizable window with a fixed 16:9 play area and letterboxing on other aspect ratios.

### Build the Windows x86_64 executable — on your Windows PC

**One menu command:** outside Play mode, select **Starfall → Build Windows x86_64**. The builder regenerates content, validates references, and uses the Mono scripting backend to build `Builds/Windows/Starfall.exe`.

**Standard Unity build dialog:**

1. Open the game scene, then **File → Build Settings**.
2. Select **PC, Mac & Linux Standalone**, target **Windows**, architecture **x86_64**. Choose **Switch Platform** if needed.
3. Verify `Assets/Starfall/Scenes/Starfall.unity` is the only enabled scene.
4. Under **Player Settings → Other Settings**, use **Mono** and the old Input Manager. Leave Development Build off for a normal release.
5. Click **Build**, choose a folder such as `Builds/Windows`, and name the executable `Starfall.exe`.
6. Run it on Windows. To share it, zip the **entire output folder**, including `Starfall_Data`, `UnityPlayer.dll`, Mono runtime folders, and any other emitted files. The `.exe` alone will not run.

Optional PowerShell command **on your Windows PC**, after downloading this project (replace the project path):

```powershell
& 'C:\Program Files\Unity\Hub\Editor\2022.3.70f1\Editor\Unity.exe' `
  -batchmode -quit -projectPath 'C:\Projects\unity-space-shooter' `
  -buildTarget Win64 -executeMethod Starfall.Editor.ProjectBuilder.BuildWindows `
  -logFile 'C:\Projects\unity-space-shooter\windows-build.log'
```

There is no WebGL build path in this project. No Linux-hosted app or public URL is involved.

### Scene, prefab, and asset workflow

| Location | Purpose |
|---|---|
| `Assets/Starfall/Scenes/Starfall.unity` | Committed bootstrap scene with stable script GUID |
| `Assets/Starfall/Scripts/` | Game state, player, enemies, bullets, pickups, HUD, stars, sparks, catalog, scene entry, pure rules |
| `Assets/Starfall/Editor/ProjectBuilder.cs` | First-import generation, menu actions, prefab wiring, reference validation, Windows build |
| `Assets/Starfall/Editor/ProceduralAssets.cs` | Deterministic analytic sprite rasterization and seeded PCM sound synthesis |
| `Assets/Starfall/Generated/` | Created by Unity: 11 PNG sprites, 6 WAV sounds, and 9 prefabs |
| `Assets/Starfall/Resources/GameAssets.asset` | Created by Unity: serialized catalog linking all generated assets |
| `Tools/` | Optional source validator and standalone rules tests; not imported into the game |

**Generated PNG/WAV/prefab/catalog files are not pre-generated in this delivery**, because Unity is unavailable here. Their complete generator source is included; Unity writes real editable assets on first import. The supplied `.unity` scene and script `.meta` files are already present.

To inspect assets, open `Generated/Prefabs` after generation. Each prefab has a sprite renderer, trigger collider, zero-gravity Rigidbody2D, and its gameplay component. Select the catalog in `Resources` to see its references. **Starfall → Validate Generated Content** checks for missing references and scripts. If the committed scene is missing, the builder recreates it through Unity's scene API.

**Rebuild Generated Content overwrites the generated assets and resets the demo build/player settings.** Keep custom artwork and prefab variants outside `Generated`, or edit the generator source. The existing scene is not overwritten. Generated assets are not git-ignored, so you can commit them after the first successful Unity import. The remaining default Unity project settings and package lock are created by the Editor.

All geometric sprites and sound formulas are original to this project, with no downloaded art, fonts, or audio. UI uses Unity's built-in skin/font. Included source and generated assets are provided under the included MIT license; the Unity engine remains subject to Unity's own terms.
