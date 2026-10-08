### Space Shooter — Unity Windows desktop

#### Open and play
1. On Windows, install **Unity 6.3 LTS (6000.3.9f1)** using Unity Hub (the release link below offers **Install with Unity Hub**). Windows Mono builds are supported by the Windows editor; install Windows Build Support if Hub offers it separately. Add/open this `space_shooter` folder as an existing project, not as a template.
2. Let Unity finish importing. `Assets/Scenes/Main.unity` opens automatically in an untouched editor; otherwise double-click that scene. Press **Play**, then click the Game view. No prefab wiring or scene setup is needed.
3. **WASD / arrows** move; hold **Space** to shoot. Each red enemy destroyed earns **10 points**. Touching an enemy or letting one pass the bottom ends the game. Click **Restart**, or press **R / Enter**. Losing window focus suspends gameplay.

#### Build Windows x86_64
1. In the Windows Unity Editor, exit Play mode. Select the custom **Space Shooter → Build Windows x64** menu. It builds the included scene using the Mono backend and Windows 64-bit target.
2. Run `Builds/Windows/SpaceShooter.exe` on Windows. Distribute the **entire `Builds/Windows` folder**, not just the executable; the data folder and Unity runtime files must stay beside it. Close the desktop window to quit.

#### Implementation and verification
- One runtime script, one editor setup/build script, a ready scene, and a small solid-colour shader referenced by the scene for build inclusion. All visible objects are built-in cubes viewed by an orthographic camera. No imported art, sound, external game packages, or manual object setup.
- Built-in Render Pipeline and legacy keyboard input (`activeInputHandler: 0`, with `com.unity.modules.inputlegacy`) are selected. IMGUI supplies the score and game-over controls. Unity generates other default editor settings and its Library on first import. Collision tests are explicit swept rectangles, not physics callbacks.
- **Local checks:** C# syntax-tree parsing, JSON/YAML parsing, asset GUID references, scene/build/input configuration, and whitespace checks. The extracted swept-rectangle collision routine is also tested in isolation with minimal mathematical types, not Unity assemblies. These checks do not verify Unity API compilation or gameplay.
- **Not tested here:** Unity import, shader compilation, Play mode, or a Windows build/executable. No executable is included. First-run check on Windows: move with both key sets, shoot a red enemy and check +10, collide/allow an escape to end the game, restart, resize the window, and repeat in the built executable.

Official references: [specified Unity release](https://unity.com/releases/editor/whats-new/6000.3.9f1), [BuildPlayer API](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html), [Windows build output files](https://docs.unity3d.com/6000.0/Documentation/Manual/WindowsStandaloneBinaries.html).
