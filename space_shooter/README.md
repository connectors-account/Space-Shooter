# Space Shooter — Unity Windows desktop source

A small, flat arcade shooter: cyan ship, red enemies, yellow bullets. Built-in square shapes and colours only; no art downloads, sound, prefabs, or external services.

### Open and run

1. Install **Unity 6.3 LTS** through Unity Hub. This project records the verified baseline **6000.3.0f1**; a newer 6000.3 LTS patch can upgrade it. Enable Windows build support for your editor installation if Unity requests it.
2. In Unity Hub, add this existing `space_shooter` folder and open it. Do not create a separate template project or copy only the scripts.
3. Wait for script compilation. The editor script creates and opens `Assets/Scenes/Game.unity`, saves its shape material, and adds the scene to the shared build scene list. No hand wiring is needed. If creation was cancelled, or another scene is open, choose **Space Shooter > Open / Create Game Scene**.
4. Press **Play**, then click the Game view for keyboard focus.

**Controls:** WASD or arrow keys move; hold Space to fire. Destroying an enemy earns 10 points. Touching an enemy ends the run immediately. Click Restart or press R / Enter on the game-over screen. Enemies that pass the bottom simply disappear. Movement stays inside the playfield; diagonal movement is normalized.

Input explicitly uses the built-in legacy Input Manager (`activeInputHandler: 0`), not the Input System package. No axis mappings are required: the script reads `KeyCode` values directly. If you change input settings, retain **Input Manager (Old)** or **Both** under Player settings / Active Input Handling and restart the editor if prompted.

### Build a Windows x86_64 executable

On your Windows machine, after opening the generated scene:

1. Open **File > Build Profiles**. Select Windows (add a Windows profile if needed), then activate/switch to that profile. Install the platform module through Unity Hub if unavailable.
2. Select **Intel 64-bit** architecture (x86_64), not ARM 64-bit. Keep the **Mono** scripting backend; the setup script selects it for Standalone. This avoids requiring an IL2CPP C++ toolchain.
3. Confirm the enabled scene list includes `Assets/Scenes/Game.unity`. If using a profile-specific scene-list override, add that scene there too.
4. Choose **Build**, select `Builds/Windows`, and name the executable `SpaceShooter.exe` if prompted.
5. Run the executable. Distribute the **entire output folder**, including its data directory and Unity DLLs, not the executable alone.

### Code

- `Assets/Scripts/SpaceShooter.cs`: runtime shapes, camera, keyboard input, bounded movement, spawning, shooting, swept bounding-box collisions, score, game-over UI, and reset.
- `Assets/Editor/SpaceShooterSetup.cs`: first-import scene/material creation and build scene registration. This editor-only script is excluded from the player.
- `ProjectSettings/ProjectSettings.asset`: explicit legacy input and desktop window defaults. Unity fills in omitted settings on import.

Collisions are computed directly in C# using swept axis-aligned bounds; no Rigidbody or physics-layer setup is needed. The scene holds a reference to the sprite material, ensuring its built-in shader is included in builds. Runtime objects are cleared on restart, and updates stop when the window loses focus.

### Validation boundary

This deliverable is **source, not a Windows executable**. Unity is not installed in the Linux authoring environment. Source syntax/project-structure checks are possible here; Unity import, C# compilation against Unity assemblies, rendering, keyboard input, gameplay, and a Windows build/run have **not** been tested. First-import serialization/module compatibility and platform build configuration must be confirmed in Unity. If the Console reports an error, preserve its full message and file/line number.

Quick Unity acceptance check: move to each screen edge; hold Space to destroy an enemy and see +10; let an enemy touch the ship; restart and confirm score is zero with no old enemies/bullets. Repeat restart, resize the window, and switch focus away/back. Repeat in the Windows build.

Official references: [6.3 LTS release baseline](https://unity.com/releases/editor/whats-new/6000.3.0f1), [Build Profiles](https://docs.unity3d.com/6000.3/Documentation/Manual/build-profiles-reference.html), [Windows architecture](https://docs.unity3d.com/6000.3/Documentation/Manual/WindowsStandaloneBinaries.html).
