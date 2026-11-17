### Space Shooter — minimal Unity Windows desktop game

A cyan triangle is your ship; red rectangles are enemies; yellow rectangles are shots. Move, shoot, and score **10 points per enemy**. Touching an enemy or letting one reach the bottom ends the game. No sound, sprites, downloaded art, prefabs, or third-party packages.

### Open and play — on your Unity machine

1. Install **Unity 6.0 LTS** through Unity Hub. This project is pinned to **6000.0.62f1**; a newer patched **6000.0 LTS** Editor is also a reasonable choice (allow Unity to upgrade its generated settings). Use the **Built-in Render Pipeline**, not URP/HDRP.
2. In Hub, choose **Add / Add project from disk**, select the downloaded `space_shooter` folder, and open it. This is already a project: do not create a template project or copy scripts into an unrelated project.
3. Open **Assets/Scenes/Main.unity**, press **Play**, then click inside the Game view. The scene contains one controller; it creates the camera, shapes, and UI automatically. The edit-time scene is intentionally empty apart from that controller.

| Control | Action |
| --- | --- |
| WASD or arrow keys | Move; diagonals are normalized |
| Hold Space | Shoot repeatedly |
| R / Enter or Restart button | Restart after game over |
| Esc | Quit the Windows application (does not exit Editor Play mode) |

Switching away from the game freezes simulation until focus returns. There is no audio.

### Build a Windows x86-64 executable — on your Windows Unity machine

1. Ensure your Editor installation supports Windows builds. If Windows is unavailable, use **Unity Hub → Installs → your Editor → Add modules** to add the Windows support offered for that installation. The project uses **Mono**, so IL2CPP / Visual Studio are not required for this build.
2. In **File → Build Profiles**, select/add **Windows**, choose **Intel 64-bit**, and **Switch Platform** if needed. Wait for compilation to finish.
3. Choose **Space Shooter → Build Windows x86-64** from the Editor menu. This selects `Main.unity`, Mono, and a resizable 960×720 window, and writes **Builds/Windows/SpaceShooter.exe**. Build failures are reported in the Console, never silently treated as success.
4. Run that executable on Windows. Distribute the **entire Builds/Windows folder**, including `SpaceShooter_Data`, `UnityPlayer.dll`, `MonoBleedingEdge`, and any other generated files—not the `.exe` alone.

Manual alternative: include only `Assets/Scenes/Main.unity` in the Build Profiles scene list, choose Windows / Intel 64-bit, set **Player → Other Settings → Scripting Backend = Mono**, then **Build** to a new folder.

### Input and implementation

The included project selects **Input Manager (Old)** under **Edit → Project Settings → Player → Other Settings → Active Input Handling**. No axes need configuration: keys are read directly. If you intentionally install Unity's Input System package and select **New** or **Both**, the conditional `ENABLE_INPUT_SYSTEM` branch reads `Keyboard.current` instead; restart the Editor after changing Active Input Handling. No Input Actions asset or UI EventSystem is needed.

- `Assets/Scripts/SpaceShooterGame.cs`: input, runtime geometry, orthographic camera, score/restart UI, focus handling, cleanup.
- `Assets/Scripts/ShooterSimulation.cs`: engine-independent movement, spawn timing, shooting, scoring, and relative swept-box collision detection. No Rigidbody/physics layers or trigger setup is required. The ship uses a slightly generous rectangular hitbox; a shot that destroys an enemy in the same step as contact saves the player.
- `Assets/Resources/FlatColor.shader`: included unlit shader; loading from Resources keeps it available in a player build.
- `Assets/Editor/WindowsBuild.cs`: explicit Windows x86-64 build menu.
- `Assets/Editor/SimulationChecks.cs`: **Space Shooter → Run Simulation Checks** runs 15 assertions in your Unity Editor, without entering Play mode.

### Verification boundary

The delivered source has passed local C# **syntax parsing** and project-reference/configuration checks. **Unity was not installed or run here. Unity API/type compilation, shader compilation, scene import, simulation checks, and Windows building/playtesting are not verified. No Windows executable is included.** The sparse project settings are completed by Unity during import; confirm the scene and the legacy input setting after import.

On your Unity/Windows machine, run the simulation-check menu, then check movement in all directions, held Space, hits and score, enemy contact, an enemy reaching the bottom, keyboard/button restart, and resizing/focus. Finally build and repeat in the executable. If the scene cannot import, the equivalent setup is an empty Built-in scene with one empty GameObject and the **SpaceShooterGame** component attached; save it as `Assets/Scenes/Main.unity`.

To repeat only the local source checks (Python, not Unity): `python -m pip install -r tools/requirements.txt`, then `python tools/check_source.py`.

### Official references

- [Unity 6000.0.62f1 release](https://unity.com/releases/editor/whats-new/6000.0.62f1)
- [Build Profiles](https://docs.unity3d.com/6000.0/Documentation/Manual/build-profiles.html)
- [Windows build settings and output files](https://docs.unity3d.com/6000.0/Documentation/Manual/WindowsStandaloneBinaries.html)
- [BuildPipeline.BuildPlayer](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html)
- [Input System installation and conditional symbols](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.17/manual/Installation.html)
