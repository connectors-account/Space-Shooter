# Space Shooter

A minimal, silent Windows desktop game for **Unity 2022.3 LTS** (project version **2022.3.62f1**), using the built-in render pipeline. Two C# scripts, one flat-colour shader, one scene. No prefabs, sprite art, external assets, or Inspector wiring.

### Open and play — on your Windows computer

1. Download the project folder. If downloaded as an archive, extract it first.
2. Install Unity Hub and **Unity 2022.3.62f1** from the Unity download archive. A newer **2022.3 LTS** patch can also open the project; accept its version upgrade if prompted. Include Windows Build Support if Hub offers it separately for your editor.
3. In Unity Hub choose **Projects → Add → Add project from disk** and select `space_shooter` (the folder containing `Assets`, `Packages`, and `ProjectSettings`). Open it in the selected editor. Let the first import finish; Unity generates its usual default settings and cache files.
4. Choose **Space Shooter → Open Game Scene**, then press **Play**. Click the Game view to give it keyboard focus. The game begins immediately. The scene is intentionally empty-looking before Play: the camera and all shapes are created at runtime.

Use the built-in pipeline, not a URP/HDRP template. This project uses the legacy keyboard API; the supplied setting is **Input Manager (Old)**. If changing settings, keep **Edit → Project Settings → Player → Other Settings → Active Input Handling** on **Input Manager (Old)** or **Both**.

### Controls and rules

| Key | Action |
| --- | --- |
| WASD or arrow keys | Move in all four directions |
| Hold Space | Shoot repeatedly |
| P | Pause / resume |
| R or Enter | Restart after game over |
| Escape | Quit the desktop executable (does not stop Unity Play Mode) |

Shoot red enemies for **10 points each**. Touching an enemy or allowing one to pass the bottom ends the run. There is one enemy type, moving straight down. Survive and increase your score; there is no final level or win screen. Losing window focus pauses the game; press P or click Resume after returning. Restart clears enemies, bullets, score, and timers. No audio is created or played.

### Build a Windows x86_64 executable — on your Windows computer

1. Stop Play Mode. Choose **Space Shooter → Build Windows x64**.
2. Save as `SpaceShooter.exe` in the suggested `Builds/Windows` folder. The helper includes `Assets/Scenes/Main.unity` and explicitly selects `StandaloneWindows64` (64-bit Windows).
3. Wait for Unity's **Build complete** dialog, then run the executable. Keep and share the **entire output folder**, including `SpaceShooter_Data`, `UnityPlayer.dll`, and any other generated files, not only the `.exe`.

Manual alternative: **File → Build Settings → PC, Mac & Linux Standalone → Windows → x86_64 → Switch Platform**. Ensure only `Assets/Scenes/Main.unity` is enabled under Scenes In Build, then click **Build** and choose an empty output folder. The project uses the Mono scripting backend, so Visual Studio's C++ toolchain is not required for this configuration.

### Code map

- `Assets/Scripts/SpaceShooter.cs`: runtime-created shapes and camera, keyboard controls, movement, shooting, spawning, bounds-based collisions, score, pause, game over, restart, and immediate-mode UI.
- `Assets/Editor/SpaceShooterSetup.cs`: scene-opening and Windows x64 build menu commands. Editor-only; excluded from the game build.
- `Assets/Resources/Flat.shader`: unlit colour shader, explicitly loaded from Resources so it is included in builds.
- `Assets/Scenes/Main.unity`: one root GameObject with the game component already attached.

### Verification status

Source syntax and project reference consistency were checked on the Linux authoring machine; see `VALIDATION.md` for the exact scope. **Unity is not installed there. Unity import, C# compilation against Unity assemblies, shader compilation, Play Mode, and the Windows executable have NOT been tested.** Static checks are not evidence of a working Unity build.

First-run acceptance check on Windows: confirm movement and diagonal speed, hold Space, destroy an enemy and see +10, touch an enemy, restart, let an enemy reach the bottom, restart again, pause/resume, resize the window, and build/run the executable. The score and controls should remain visible at the top of the letterboxed 16:9 playfield.
