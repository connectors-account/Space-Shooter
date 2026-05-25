# Build Instructions — Space Shooter (Unity 2D)

Complete step-by-step guide to open, configure, and build this project into a **standalone Windows executable (.exe)**.

---

## Prerequisites

| Requirement | Details |
|-------------|---------|
| **Unity Hub** | Download from [unity.com/download](https://unity.com/download) |
| **Unity Editor** | Version **2021.3 LTS** or newer (any 2021.3.x, 2022.x, or 2023.x works) |
| **Build Support** | Install the **Windows Build Support (Mono)** module via Unity Hub |
| **OS** | Windows 10/11 (for building). macOS/Linux can build for Windows via Unity too |
| **Disk Space** | ~3 GB for Unity + ~500 MB for project |

### Installing Unity

1. Download and install **Unity Hub** from [unity.com/download](https://unity.com/download)
2. Sign in or create a Unity account (free Personal license is fine)
3. In Unity Hub, go to **Installs** → **Install Editor**
4. Select **Unity 2021.3 LTS** (or newer LTS version)
5. In the module selection, ensure **Windows Build Support (Mono)** is checked
6. Click **Install** and wait for completion

---

## Step 1: Open the Project in Unity

1. Open **Unity Hub**
2. Click **Open** (or **Add project from disk**)
3. Navigate to and select this project's root folder:
   ```
   space_shooter_game/
   ```
4. Unity Hub will detect the project. Click to open it
5. If prompted about Unity version mismatch, click **Continue** — the project is forward-compatible
6. Wait for Unity to import all assets (first time may take 1–3 minutes)

---

## Step 2: Configure Tags (Critical!)

The game uses custom tags for collision detection. Verify they exist:

1. Go to **Edit → Project Settings → Tags and Layers**
2. Expand the **Tags** section
3. Ensure these tags are listed:
   - `Enemy`
   - `PlayerBullet`
   - `EnemyBullet`
   - `PowerUp`
4. If any are missing, click **+** to add them with the exact names above

> **Note:** The `TagManager.asset` file included should auto-configure these, but always verify.

---

## Step 3: Set Up the Game Scene

The game uses a **self-bootstrapping** architecture — one script creates everything at runtime.

### Option A: Quick Setup (Recommended)
1. Go to **File → New Scene → Basic 2D** (or just use the default scene)
2. Save it: **File → Save As** → `Assets/Scenes/MainScene.unity`
3. In the **Hierarchy** window, right-click → **Create Empty**
4. Name the new GameObject: `GameSetup`
5. In the **Project** window, navigate to `Assets/Scripts/`
6. Drag `GameSetup.cs` onto the `GameSetup` object in the Hierarchy
7. **Done!** — All game objects are created automatically when you press Play

### Option B: If You Want More Control
You can manually set up objects instead. See `Assets/Scenes/SceneSetupInstructions.md` for the full manual setup guide.

---

## Step 4: Test in the Editor

1. Click the **Play ▶** button at the top of the Unity Editor
2. You should see:
   - A dark space background with scrolling stars
   - The **Main Menu** with "PLAY" and "QUIT" buttons
3. Click **PLAY** to start the game
4. Controls:
   - **Arrow Keys** or **WASD** — Move the ship
   - **Space** — Shoot
5. Enemies will spawn in waves with increasing difficulty
6. Collect power-ups (colored diamonds) for shields, rapid fire, multi-shot, or health
7. Press **Play ▶** again to stop testing

---

## Step 5: Configure Build Settings

1. Go to **File → Build Settings** (or `Ctrl+Shift+B`)
2. Set the **Platform** to **PC, Mac & Linux Standalone** (should be default)
3. Set **Target Platform** to **Windows**
4. Set **Architecture** to **x86_64** (64-bit)
5. Click **Add Open Scenes** to add your MainScene to the build
   - Ensure `Scenes/MainScene` appears in the scene list with index 0
6. Click **Player Settings...** to configure:

### Player Settings (optional but recommended)
- **Company Name**: Your name or studio
- **Product Name**: `Space Shooter`
- **Default Screen Width**: `1280`
- **Default Screen Height**: `720`
- **Fullscreen Mode**: `Windowed` (or `Fullscreen Window`)
- **Run In Background**: ✓ Checked
- **Resolution Dialog**: `Disabled` (for cleaner startup)

---

## Step 6: Build the Executable

1. In **Build Settings**, click **Build** (or **Build And Run**)
2. Choose a destination folder (e.g., create a folder called `Build/`)
3. Name the executable: `SpaceShooter.exe`
4. Click **Save** and wait for the build to complete (1–3 minutes)
5. When finished, the build folder will contain:
   ```
   Build/
   ├── SpaceShooter.exe              ← The game executable
   ├── SpaceShooter_Data/            ← Game data folder (required)
   │   ├── Managed/
   │   ├── Resources/
   │   └── ...
   ├── UnityPlayer.dll               ← Unity runtime (required)
   └── MonoBleedingEdge/             ← Mono runtime (required)
   ```

---

## Step 7: Run the Game

1. Navigate to your Build folder
2. Double-click `SpaceShooter.exe`
3. The game will launch in a window
4. Play!

### Distribution
To share the game with others, **zip the entire Build folder** — all files are needed:
- `SpaceShooter.exe`
- `SpaceShooter_Data/` folder
- `UnityPlayer.dll`
- `MonoBleedingEdge/` folder

---

## Troubleshooting

### "Tags not found" / Objects not colliding
→ Go to Edit → Project Settings → Tags and Layers and add all four custom tags (see Step 2)

### "No cameras rendering" warning
→ The default Main Camera should be present in the scene. If deleted, create one:
- Right-click Hierarchy → Camera. Set it to Orthographic, Size 5, Position (0, 0, -10)

### UI text not appearing / blank text
→ Ensure `com.unity.ugui` is in the Package Manager (Window → Package Manager → Unity Registry → search "UI")

### Build fails with "No scenes in build"
→ In Build Settings, click "Add Open Scenes" with MainScene open

### Player doesn't move
→ Check that the GameSetup script is attached to a GameObject in the scene and tags are configured

### Enemies don't die when shot
→ Verify the "Enemy" tag exists and is properly assigned. The runtime prefab creator sets this automatically

### No sound effects
→ Audio is generated procedurally — check that your system volume is up and Unity audio isn't muted

---

## Project Architecture

```
Assets/
├── Scripts/
│   ├── GameSetup.cs            ← Master bootstrap (creates all objects)
│   ├── PlayerController.cs     ← Player movement, shooting, health
│   ├── EnemyController.cs      ← Enemy AI, movement patterns, shooting
│   ├── BulletController.cs     ← Bullet movement and collision
│   ├── EnemySpawner.cs         ← Wave-based enemy spawning
│   ├── PowerUpController.cs    ← Power-up types and pickup logic
│   ├── GameManager.cs          ← Central game state (singleton)
│   ├── UIManager.cs            ← All UI (menus, HUD, game over)
│   ├── ParallaxBackground.cs   ← Scrolling star background
│   ├── AudioManager.cs         ← Procedural sound effects
│   ├── SpriteFactory.cs        ← Runtime geometric sprite generation
│   └── ExplosionEffect.cs      ← Visual explosion animation
├── Scenes/
│   └── MainScene.unity         ← (Created by you in Step 3)
ProjectSettings/
│   ├── TagManager.asset        ← Custom tags (Enemy, etc.)
│   ├── InputManager.asset      ← WASD/Arrow/Space controls
│   ├── Physics2DSettings.asset ← Zero gravity for space
│   └── ...
```

---

## Command-Line Build (Advanced)

You can build from the command line without opening the Unity Editor:

```bash
# Windows (PowerShell)
& "C:\Program Files\Unity\Hub\Editor\2021.3.0f1\Editor\Unity.exe" `
  -batchmode -nographics -quit `
  -projectPath "C:\path\to\space_shooter_game" `
  -buildWindows64Player "C:\path\to\Build\SpaceShooter.exe"

# macOS/Linux
/path/to/Unity -batchmode -nographics -quit \
  -projectPath "/path/to/space_shooter_game" \
  -buildWindows64Player "/path/to/Build/SpaceShooter.exe"
```

> **Note:** Command-line builds require a valid Unity license activated on the machine.
