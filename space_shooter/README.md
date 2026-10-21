# Space Shooter — Unity Windows desktop source

Minimal top-down shooter using Unity cube primitives, flat colours and an orthographic camera. No artwork, sound, third-party packages, web app or server.

### Open and play on your Windows PC

1. Install **Unity Hub** and **Unity 2022.3.62f1 LTS**, including **Windows Build Support (Mono)** if Hub offers it separately. This project uses the **Built-in Render Pipeline**, not URP/HDRP. A newer 2022.3 LTS patch can upgrade the project, but was not tested here.
2. Download the whole `space_shooter` folder. In Hub choose **Projects → Add → Add project from disk** and select that folder (the one containing `Assets`, `Packages` and `ProjectSettings`). Open it with the installed editor. Let Unity import and generate its local settings.
3. Choose **Space Shooter → Open Game Scene**, then press **Play** and click the Game view to focus it. The scene creates its camera, player, enemies and UI automatically. Do not attach scripts or assign Inspector fields.

The setup script registers `Assets/Scenes/Main.unity` as the build scene and configures a resizable 960 × 720 desktop window. Input uses Unity's built-in legacy keyboard API; if changing settings manually, keep **Edit → Project Settings → Player → Other Settings → Active Input Handling** at **Input Manager (Old)** or **Both**.

### Controls and rules

- **WASD or arrow keys:** move; diagonal speed is normalized and the ship stays inside the field.
- **Hold Space:** fire upward.
- Each destroyed red enemy gives **10 points**. Enemies move straight downward.
- Touch an enemy, or let one escape below the screen, and the game ends.
- Click **Restart** to clear the field and reset the score. Close the desktop window to quit.

### Build the Windows x86_64 executable

On your Windows PC, outside Play mode:

1. Choose **File → Build Settings**. Select **PC, Mac & Linux Standalone**, **Target Platform: Windows**, **Architecture: Intel 64-bit**, and click **Switch Platform** if needed.
2. Choose **Space Shooter → Build Windows x86_64**. The menu uses the Mono backend and the registered main scene. A failed build reports an error in the Unity Console instead of reporting success.
3. After Unity reports success, run `Builds/Windows/SpaceShooter.exe`.
4. To share the game, zip and send the **entire `Builds/Windows` folder**, including `SpaceShooter_Data`, `UnityPlayer.dll`, Mono runtime folders and all other generated files. The `.exe` alone is not enough. Recipients do not need Unity installed.

No Windows executable is included in this source delivery.

### Complete source

- `Assets/Scripts/SpaceShooterGame.cs` — startup, primitive models, movement, firing, spawning, swept collisions, score, UI and restart.
- `Assets/Scripts/GameRules.cs` — pure C# collision and boundary math.
- `Assets/Editor/DesktopProject.cs` — automatic project setup and Windows build menu; excluded from the player build.
- `Assets/Resources/ShipColor.shader` — small solid-colour shader for the Built-in pipeline. Keeping it in Resources ensures inclusion in the build; no lighting or textures required.
- `Assets/Scenes/Main.unity` — starter scene; game objects are generated on entering Play mode.
- `Tests/GameRulesTests.cs` — standalone collision/boundary checks, outside Assets so they are not compiled into the game.

### Verification boundary

The Linux checks compile and run the **pure C# rules only**, and check C# syntax and project-file consistency. They do **not** compile against Unity assemblies or validate Unity scene import, shader compilation, rendering, keyboard/GUI interaction, game-loop behaviour or a Windows player build. Unity Editor is unavailable on this machine. Actual Play-mode testing and the Windows build remain to be performed using the steps above. See `VALIDATION.md` for the recorded results.

Optional rules-test command on a machine with Mono installed (run from the project root):

```sh
mcs -warnaserror -out:/tmp/SpaceShooterRulesTests.exe Assets/Scripts/GameRules.cs Tests/GameRulesTests.cs
mono /tmp/SpaceShooterRulesTests.exe
```

Before distributing your Windows build, verify movement in all directions, sustained shooting and scoring, collision and escape game-over, Restart resetting all objects and score, and window resizing.
