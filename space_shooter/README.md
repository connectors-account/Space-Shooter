### Space Shooter — Unity / Windows

Small 2D desktop arcade game: cyan player, yellow shots, red downward-moving enemies. Destroy an enemy for 10 points. Touching an enemy **or letting one escape below the screen** ends the game. Restart resets the score and clears all enemies and shots. No audio, imported artwork, networking, or third-party packages.

#### Open and play

1. Install **Unity 6.0 LTS, 6000.0.62f1**, through Unity Hub. This is the pinned project version, not a claim that it is the latest release.
2. In Hub choose **Add → Add project from disk**, select this `space_shooter` folder, and open it with that editor. Allow Unity to import and create its local settings/cache files.
3. Open `Assets/Scenes/Main.unity`, press **Play**, and click the Game view to give it keyboard focus.

The scene intentionally contains only a root object in edit mode. The camera, flat-colour rectangular shapes, game controller, and interface are created automatically on Play. No components need attaching and no Inspector references need setting. The project uses the built-in render pipeline, legacy keyboard input, and built-in IMGUI, not URP, TextMesh Pro, or a Canvas.

| Control | Action |
| --- | --- |
| WASD or arrow keys | Move; diagonals have the same speed |
| Hold Space | Shoot upwards |
| R or Enter after game over | Restart |
| Restart button | Restart after game over |
| Window close button / Alt+F4 | Quit the Windows game |

#### Build a Windows x86_64 executable

Use the **Windows Unity Editor** for the intended build workflow:

1. In Unity Hub, install **Windows Build Support (Mono)** for the selected editor if it is offered separately. On a Windows editor installation, Windows Mono support may already be included. This project uses **Mono**, not IL2CPP; Windows Build Support (IL2CPP) is not required for this workflow.
2. Open the project and wait for script import to finish with no Console errors.
3. Choose **Space Shooter → Build Windows x86_64**. Select an empty output folder (preferably outside the source project). The included editor script explicitly builds `Assets/Scenes/Main.unity` for `StandaloneWindows64` and selects Mono.
4. Run `SpaceShooter.exe` on Windows. To share it, zip **the entire output folder**, including the EXE, `SpaceShooter_Data`, `UnityPlayer.dll`, and any other generated runtime files. The EXE alone is not sufficient.

Manual alternative: **File → Build Profiles**, add/select the **Windows** platform and **Switch Profile** if necessary; use the **Intel 64-bit / x86_64** architecture, the **Mono** scripting backend in Player Settings, and ensure `Assets/Scenes/Main.unity` is enabled in the scene list. Choose **Build**, not Create Visual Studio Solution.

#### Source map

- `Assets/Scripts/SpaceShooterGame.cs`: runtime bootstrap, movement, shots, enemy spawning, swept rectangle collisions, score and game-over/restart UI.
- `Assets/Editor/WindowsBuild.cs`: Windows x86_64 build menu; excluded from player compilation by its `Editor` folder.
- `Assets/Scenes/Main.unity`: ready-to-open entry scene, already enabled in build settings.
- `ProjectSettings`: pinned editor, window/input settings and build scene list. Unity generates unspecified default settings when importing.
- `Packages/manifest.json`: only Unity's built-in IMGUI and legacy-input modules.

#### Verification and limits

**Unity is not installed in the authoring environment. Unity import/compilation, Play mode, scene deserialization and a Windows build/run have NOT been verified.** This is a complete source project, not a prebuilt EXE. No Unity installation was attempted.

`Tools/check_source.py` checks C# syntax with tree-sitter, package JSON, scene/build GUID links, asset metadata, input mode, and runtime/editor separation. These are source consistency checks, **not Unity compiler or gameplay tests**. To repeat those checks with Python: `python -m pip install -r Tools/requirements-checks.txt`, then `python Tools/check_source.py`.

First Unity verification: confirm the Console is clear; play and test movement at all four edges, Space firing, +10 per kill, loss by collision and by a missed enemy, then both keyboard and button restarts. Build on Windows and repeat in the EXE, including resizing the window.

Unity references: [pinned editor release](https://unity.com/releases/editor/whats-new/6000.0.62f1), [Build Profiles](https://docs.unity3d.com/6000.0/Documentation/Manual/BuildSettings.html), [Windows build settings](https://docs.unity3d.com/6000.0/Documentation/Manual/WindowsStandaloneBinaries.html), [runtime initialization](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/RuntimeInitializeOnLoadMethodAttribute.html).
