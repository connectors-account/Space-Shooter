# Space Shooter — Unity Windows desktop

Minimal 2D shooter: cyan player, red enemies, yellow bullets. All shapes are runtime-created coloured rectangles; no artwork, audio, prefabs or third-party assets. Uses Unity 2D trigger colliders and rigidbodies. Each destroyed enemy scores one point. Contact with an enemy, or an enemy leaving the bottom of the arena, ends the game.

### Open and play (on your Windows PC)
1. Install **Unity 2022.3 LTS** through Unity Hub (project baseline: **2022.3.62f1**; a newer 2022.3 patch is also appropriate). Use the **Built-in Render Pipeline**, not URP/HDRP. This folder is already a minimal project: Hub → **Add / Add project from disk** → select `space_shooter` → open it. Unity generates its Library and remaining default settings on first import.
2. Open `Assets/Scenes/Main.unity`. Under **Edit → Project Settings → Player → Other Settings → Configuration**, confirm **Active Input Handling = Input Manager (Old)** (or **Both**), not new Input System only. Restart the Editor if prompted. No input axes need configuring: the scripts read keys directly.
3. Press **Play** and click the Game view to give it keyboard focus. The scene is intentionally almost empty in Edit mode; the runtime bootstrap creates the camera, player, enemies and UI automatically. No script attachment or Inspector wiring is needed.

If importing into a separate project instead: create a **2D Core / Built-in** project with Unity 2022.3, copy the supplied `Assets` folder into it, open `Main.unity`, apply the input setting above, and include that scene in Build Settings.

### Controls
- **WASD / arrow keys:** move (diagonal speed is normalized; movement stays in the arena).
- **Hold Space:** shoot upward.
- **R** or **Restart button:** restart after game-over; score resets to zero.
- **Esc:** quit the built desktop application. In the Editor, use the Play button to stop.

### Build a Windows executable (on your Windows PC)
1. Stop Play mode. Open **File → Build Settings**. Confirm `Assets/Scenes/Main.unity` is enabled in **Scenes In Build** (already supplied); otherwise open it and click **Add Open Scenes**.
2. Select **PC, Mac & Linux Standalone**, set **Target Platform = Windows**, **Architecture = x86_64**, then **Switch Platform** if offered. If Windows support is missing, install it using Unity Hub → Installs → your editor → Add modules.
3. Under **Player Settings → Other Settings**, choose **Scripting Backend = Mono**. Under **Resolution and Presentation**, use **Windowed**, **960 × 720**, and **Resizable Window** (supplied defaults). IL2CPP/Visual Studio is not required for this Mono build.
4. Click **Build**, choose a folder such as `Builds/Windows`, and save as `SpaceShooter.exe`. Launch that executable. Distribute the **entire output folder**, including `SpaceShooter_Data`, `UnityPlayer.dll` and any other generated files—not just the `.exe`.

### Source and verification
All complete gameplay source is in `Assets/Scripts`: `SpaceShooter.cs` (bootstrap, spawning, scoring, UI/restart), `PlayerShip.cs` (movement/shooting), `Projectile.cs` (bullet collision), `EnemyShip.cs` (descent/player collision) and `ArenaViewport.cs` (fixed-aspect arena).

This delivery is **source only, not a compiled executable**. C# syntax parsing and project-file/static consistency checks were performed on Linux. **Unity import, Unity compilation, physics/rendering behavior, Windows builds and Windows runtime have not been tested here.** Before distributing, verify movement and shooting, exactly one point per enemy, both game-over paths, repeated restarts, resizing and quitting in Unity and the Windows build.
