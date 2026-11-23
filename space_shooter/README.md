### Space Shooter

A tiny Windows desktop Unity game: move, hold Space to shoot, and survive. One enemy type moves straight down. Each destroyed enemy scores **10 points**. A collision or any enemy reaching the bottom ends the game. Restart with the button, **R**, or **Enter**.

#### Open and run (on your Windows computer)

1. Install **Unity Hub** and **Unity 6.3, editor `6000.3.0f1`**, the stable release pinned by this project. It is not a claim to be the latest patch. [Official editor download](https://unity.com/releases/editor/whats-new/6000.3.0f1).
2. Download the complete `space_shooter` directory. In Hub, choose **Add → Add project from disk** and select it (the directory containing `Assets`, `Packages`, and `ProjectSettings`). Do not create a new project or copy only the script.
3. Open it using the specified editor and let Unity import it. Unity generates the remaining default settings and local cache on first open.
4. Open **Assets/Scenes/Main.unity**, press **Play**, and click the **Game** view to give it keyboard focus. No inspector wiring or prefab setup is needed.

**Controls:** WASD or arrow keys move in four directions; hold Space to fire. R / Enter restart after game over. Escape quits the standalone application (it does not exit Play Mode in the editor).

#### Build a Windows executable (on your Windows computer)

1. In **File → Build Profiles**, select/add **Windows**, choose **Intel 64-bit / x86_64** architecture and **Switch Platform** if needed. If Unity reports missing Windows build support, install that module for this editor through Hub.
2. In **Player Settings → Other Settings**, use **Mono** as the scripting backend (already configured). Under **Active Input Handling**, keep **Input Manager (Old)**, not **Input System Package (New)**.
3. In the build profile's **Scene List**, confirm `Assets/Scenes/Main.unity` is enabled. It is already in the project's shared build scene list; disable any profile-specific scene override that excludes it.
4. Choose **Build** or **Build And Run**, create `Builds/Windows`, and name the output `SpaceShooter.exe`.
5. To share the game, distribute the **entire build output folder**, including the executable, `SpaceShooter_Data`, `UnityPlayer.dll`, and any other files Unity produces. The `.exe` is not self-contained.

#### Implementation

- **Assets/Scripts/SpaceShooter.cs** contains the complete game. The supplied scene has one object with this component attached. `Awake` creates the background camera; `Update` owns movement, spawning, shots, collisions, score and restart state.
- Built-in **IMGUI** draws coloured rectangular ships, projectiles, score and the restart button. There are no imported sprites, fonts, audio, external assets, prefabs or render pipelines. This is intentionally a flat, minimal 2D game, not a 3D scene.
- Keyboard input uses `UnityEngine.Input.GetKey/GetKeyDown`. The package manifest explicitly enables **inputlegacy**, **imgui**, and **textrendering** (for built-in GUI text), and `activeInputHandler: 0` selects the legacy input backend. There is no Input System or uGUI dependency, input action asset, Canvas or EventSystem. Named input axes are unnecessary.
- Collision checks use logical `Rect` bounds rather than physics. Shots use swept rectangles, including enemy travel, to avoid skipped hits. Diagonal movement is normalized and the player stays inside the arena.
- The logical arena is 960 × 720 and is letterboxed when resized. Speeds use delta time; long stalls are capped to 1/30 second per update, deliberately slowing simulation rather than letting entities jump. Losing focus stops simulation updates. Restart clears shots, enemies, timers and score.

#### Verification status

**Unity compilation, editor import, Play Mode, and Windows build/run have NOT been tested. No Windows executable is included.** The Linux preparation environment has no Unity editor or C# compiler. The deliverable is the complete source project, not a verified binary.

Available checks parse C# syntax and inspect package/input settings, scene references, metadata GUIDs and build scene registration. They do **not** resolve Unity APIs, compile assemblies, import the Unity scene or test gameplay. To repeat these optional developer checks outside Unity:

```text
python -m pip install tree-sitter==0.25.2 tree-sitter-c-sharp==0.23.1 PyYAML==6.0.3
python Tools/check_project.py
```

First Unity acceptance pass: verify the Console has no errors; check WASD/arrows and diagonal movement; hold Space to hit a red enemy and see +10; touch an enemy or let it cross the bottom and see Game Over; restart by button and by R / Enter and confirm score and entities reset; resize the window and check letterboxing; build Windows and repeat, including Escape to quit. Remaining integration risks are first-import/default-settings serialization and Unity API/module resolution, which require the actual editor to confirm.
