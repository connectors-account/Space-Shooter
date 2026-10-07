# Space Shooter — Unity Windows desktop

### Open and play
1. On Windows, install **Unity 6.0 (6000.0.23f1)** with Unity Hub and its Windows Build Support module if offered. Add/open this `space_shooter` folder as an existing project (not as a template).
2. Let Unity finish importing. `Assets/Scenes/Main.unity` opens automatically in an untouched editor; otherwise double-click that scene. Press **Play**, then click the Game view.
3. **WASD / arrows** move; hold **Space** to shoot. Each red enemy destroyed earns **10 points**. Touching an enemy or letting one pass the bottom ends the game. Click **Restart**, or press **R / Enter**. Losing window focus suspends gameplay.

### Build Windows x86_64
1. Exit Play mode. Select the project's custom **Space Shooter → Build Windows x64** menu. It builds the included scene using the Mono backend and the Windows 64-bit target.
2. Run `Builds/Windows/SpaceShooter.exe` on Windows. Distribute the **entire `Builds/Windows` folder**, not just the executable; the data folder and Unity runtime files must stay beside it.

### Implementation and verification
- One runtime script, one editor setup/build script, a ready scene, and a small solid-colour shader (referenced by the scene so it is included in builds). All visible objects are built-in cubes. No imported art, sound, external game packages, or manual object setup.
- Built-in Render Pipeline and legacy keyboard input are configured. Unity generates other default editor settings and its Library on first import. Collision tests are explicit swept rectangles, not physics callbacks.
- **Source-only validation:** C# syntax-tree parsing, JSON/YAML parsing, asset GUID references, scene/build/input configuration, and whitespace checks. These do not verify Unity API compilation or gameplay.
- **Not tested here:** Unity import, shader compilation, Play mode, or a Windows build/executable. No executable is included. First-run check on Windows: move with both key sets, shoot a red enemy and check +10, collide/allow an escape to end the game, restart, resize the window, and repeat in the built executable.

Official references: [specified Unity release](https://unity.com/releases/editor/whats-new/6000.0.23f1), [BuildPlayer API](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html), [Windows build output files](https://docs.unity3d.com/6000.0/Documentation/Manual/WindowsStandaloneBinaries.html).
