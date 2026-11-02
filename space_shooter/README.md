# Space Shooter

A small **Unity Windows desktop game**, not a web app. Two complete C# scripts; no downloaded art, audio, third-party packages, prefabs, or physics setup. Coloured rectangles are drawn with Unity's built-in white texture and IMGUI. Rectangle collisions run in the game controller.

## Open and play

1. In Unity Hub, install **Unity 2022.3 LTS** with **Windows Build Support (Mono)** on your Windows computer. The project records **2022.3.62f1**; another 2022.3 LTS patch may prompt a project upgrade.
2. Use **Add / Add project from disk**, choose this `space_shooter` folder, and open it. Allow Unity to import and generate its remaining default project settings.
3. The included editor script automatically creates `Assets/Scenes/Main.unity` and adds it to Build Settings. Choose **Space Shooter > Open Game Scene**, then press **Play**. No objects need assembling. Pressing Play in an empty scene also starts the game automatically.
4. Click inside the Game view so it receives keyboard input.

**Input:** uses Unity's built-in legacy `UnityEngine.Input` API, not the Input System package. A fresh project uses this by default. If reusing these scripts in another project, set **Edit > Project Settings > Player > Other Settings > Active Input Handling** to **Input Manager (Old)** or **Both**, and restart the editor if prompted. No input axes need configuring; keys are read directly.

## Controls and rules

- **WASD / arrow keys:** move; diagonal speed is normalized and movement stays inside the field.
- **Hold Space:** shoot upward.
- Cyan rectangle: player. Yellow rectangles: shots. Red rectangles: enemies moving downward.
- Each enemy destroyed gives **10 points**.
- Touching an enemy, or letting any enemy reach the bottom, ends the game.
- **R** or **Restart** resets the score, player, enemies, bullets, and timers after game over.
- Close the window normally to quit. No sound, menus, levels, pickups, or extra systems.

The game uses a fixed 960 × 600 logical field, scaled to fit the window with black margins. There are no physical units or simulated rigid bodies. Background focus loss stops updates; gameplay speed slows during severe stalls instead of taking a large collision step.

## Build a Windows executable

On your **Windows computer**, with the project open in Unity:

1. Open **File > Build Settings**. Confirm `Assets/Scenes/Main` is checked under Scenes In Build. If missing, open it through **Space Shooter > Open Game Scene**, then click **Add Open Scenes**.
2. Select **PC, Mac & Linux Standalone**, **Target Platform: Windows**, **Architecture: x86_64**, then **Switch Platform** if needed.
3. Setup defaults to **Mono** and a resizable **960 × 600 window**. Leave those defaults, click **Build**, and save to `Builds/Windows/SpaceShooter.exe`.
4. Run `SpaceShooter.exe`. When sharing it, include **all files and folders from that build output**, not just the `.exe`.

## Source

- `Assets/Scripts/SpaceShooter.cs`: complete game, bootstrap, movement, firing, spawning, collisions, scoring, restart, and display.
- `Assets/Editor/SpaceShooterSetup.cs`: automatic scene creation and desktop defaults; excluded from player builds by Unity's `Editor` folder convention.
- `Packages/manifest.json`: only Unity's built-in IMGUI module; no third-party dependencies.
- `ProjectSettings/ProjectVersion.txt`: Unity editor version hint.

## Validation status

Both scripts passed C# syntax-tree parsing on Linux (not compilation). Checks also passed for project layout, JSON validity, required gameplay paths, reset assignments, and editor/runtime separation. Rectangle overlap, swept-shot collision, movement bounds, diagonal speed, and the bottom boundary were checked independently; these checks do **not** execute the C# game.

**Unity compilation, scene generation inside Unity, visual rendering, keyboard/mouse interaction, and Windows execution have NOT been tested here. No `.exe` has been built.** No Unity tooling was installed. Open in Unity to verify those integration steps.

Quick local acceptance check: move in each direction and into the borders; hold Space and confirm kills add 10; let an enemy reach the bottom; restart with R and with the button; collide with an enemy; resize the window; build and run on Windows. Likely integration points to check if import fails are the Unity version, legacy input setting, and installed Windows build module.
