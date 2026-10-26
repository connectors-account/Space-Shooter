### Space Shooter

A minimal, silent Windows desktop game written in C# for **Unity 6.0 LTS**, using the **Built-in Render Pipeline**. The project records editor version `6000.0.23f1`; a newer Unity 6.0 LTS patch can upgrade it when opening. No third-party packages, downloaded art, prefabs, or manual Inspector wiring are required. The only package entries are Unity's built-in IMGUI, legacy input, and physics modules.

#### Open and play — on your Windows computer

1. Install Unity Hub and a Unity 6.0 LTS editor on Windows. The Windows editor normally includes Mono Windows build support; install any additional **Windows Build Support** module requested by Hub.
2. Download/extract this project. In Unity Hub, choose **Add → Add project from disk** and select `space_shooter`, the folder containing `Assets`, `Packages`, and `ProjectSettings`.
3. Open it and allow the first import to finish. Accept a Unity 6.0 patch upgrade if prompted; Unity generates its normal cache and remaining default settings.
4. Choose **Space Shooter → Configure Project**, then **Space Shooter → Open Game Scene**. Press **Play**, then click the Game view to focus it.

The bootstrap creates the orthographic camera, ship, enemies, bullets, materials, and HUD at runtime. The included scene intentionally has no manually wired gameplay objects. Keep **Active Input Handling = Input Manager (Old)** or **Both** under Player Settings; the supplied settings select Old. Do not convert this project to URP/HDRP.

#### Controls and rules

- **WASD / arrow keys:** move in all four directions; diagonals are normalized and movement stays inside the arena.
- **Hold Space:** fire upward.
- **R** or **Restart:** restart after game over, clearing enemies, bullets, and score.
- **Esc:** quit the built desktop game. In the editor, stop Play mode normally.
- Losing window focus pauses gameplay; refocusing resumes it.

Red enemies spawn at the top and move straight down. Each destroyed enemy earns **10 points**. Contact with an enemy ends the run. Enemies that leave the bottom are removed without a score or life penalty. There is no end-of-level or win condition: survive and improve your score. Collision uses swept axis-aligned bounds, not Unity physics callbacks, to catch fast bullets between frames. Bullet hits resolve before surviving enemies can hit the player in that frame.

#### Build Windows x86_64 — on your Windows computer

1. Stop Play mode and choose **Space Shooter → Build Windows x86_64**.
2. Choose a dedicated output folder, preferably `Builds/Windows` inside the project. The helper sets the scene, window options, and Mono backend, then builds `SpaceShooter.exe` for `StandaloneWindows64`.
3. Run `SpaceShooter.exe`. Distribute the **entire output folder**, including `SpaceShooter_Data`, `UnityPlayer.dll`, and other generated files—not just the executable.

Alternatively, use **File → Build Profiles → Windows**, choose **Intel 64-bit / x86_64**, ensure `Assets/Scenes/Main.unity` is included, and build. Install any missing build module through Unity Hub. A licensed/activated Unity installation is required even for a batch build.

#### Source map

- `Assets/Scripts/SpaceShooterGame.cs`: runtime setup, keyboard input, spawning, collision processing, score, HUD, game-over state, and restart.
- `Assets/Scripts/GameRules.cs`: pure C# movement limits, tuning constants, and swept collision math.
- `Assets/Resources/SolidColor.shader`: solid-colour unlit shader, included through Resources so player builds retain it.
- `Assets/Editor/SpaceShooterSetup.cs`: scene/configuration menus and Windows build helper; excluded from the player.
- `Tests/GameRulesTests.cs`: standalone rules tests, outside Unity's Assets folder; no test-package dependency.

#### Verification scope

See `VERIFICATION.md` for the checks actually run. The Linux development machine does **not** have Unity installed. Unity import, Unity API compilation, shader compilation, visual/gameplay testing, and a Windows executable build have **not** been verified here. Pure C# rule checks and syntax checks are not a substitute for those steps.

Before distributing, test in Unity and in the Windows build: all movement keys and arena edges, held-fire behavior, a destroyed enemy adding 10 points, player contact causing game over, R and button restart, resizing, focus loss/resume, and Escape exit.

Optional rule-test commands on any machine with Mono installed (not needed to open/build in Unity):

```sh
mcs -out:/tmp/SpaceShooterRulesTests.exe Assets/Scripts/GameRules.cs Tests/GameRulesTests.cs
mono /tmp/SpaceShooterRulesTests.exe
```
