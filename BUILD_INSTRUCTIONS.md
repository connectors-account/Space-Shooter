# Space Shooter - Complete Build Instructions for Windows

## Prerequisites

1. **Unity Hub** - Download from: https://unity.com/download
2. **Unity Editor** - Version **2021.3 LTS** or newer (2022.3 LTS recommended)
   - During install, ensure **Windows Build Support (IL2CPP)** module is included
   - Also install **Windows Build Support (Mono)** as a fallback
3. **Windows 10/11** (64-bit) for building

---

## Step-by-Step Setup

### Step 1: Create a New Unity Project

1. Open **Unity Hub**
2. Click **"New Project"**
3. Select the **"2D (Built-in Render Pipeline)"** template
4. Name it `SpaceShooter`
5. Choose your preferred location
6. Click **"Create Project"**

### Step 2: Copy Project Files

1. Close Unity (or keep it open - it will auto-refresh)
2. Copy the contents of this project into your Unity project:

```
# Copy all script files:
Copy the entire Assets/ folder contents INTO your Unity project's Assets/ folder:
  - Assets/Scripts/       → YourProject/Assets/Scripts/
  - Assets/Editor/        → YourProject/Assets/Editor/
```

If Unity is open, it will automatically detect and import the new files.

**Alternatively**, you can replace the entire Assets folder:
```
# Delete the existing Assets folder in your Unity project
# Copy our Assets folder into the Unity project root
```

### Step 3: Set Up the Game Scene (One-Click Method)

1. Open Unity and wait for scripts to compile (check bottom-right for progress)
2. In the menu bar, go to: **Tools → Space Shooter → Create New Scene and Setup**
3. This automatically:
   - Creates a new scene
   - Sets up the camera (orthographic, dark space background)
   - Adds the GameBootstrapper (which creates everything else at runtime)
   - Saves the scene to `Assets/Scenes/GameScene.unity`

### Step 4: Test in Editor

1. Press the **Play** button (▶) in the Unity Editor
2. You should see:
   - **Main Menu** with "SPACE SHOOTER" title
   - Click **"START GAME"** to begin playing
   - Stars scrolling in the background
   - Your blue player ship at the bottom
3. Controls:
   - **WASD** or **Arrow Keys**: Move
   - **Space**: Shoot
   - **ESC**: Pause

### Step 5: Configure Build Settings

#### Automatic Method:
1. Go to: **Tools → Space Shooter → Configure Build Settings**
2. This sets resolution, window mode, and build target automatically

#### Manual Method:
1. Go to **File → Build Settings**
2. Click **"Add Open Scenes"** to add GameScene
3. Select **"PC, Mac & Linux Standalone"** as platform
4. Click **"Switch Platform"** if needed
5. Under **Target Platform**, select **"Windows"**
6. Set **Architecture** to **"x86_64"**

#### Player Settings:
1. Click **"Player Settings..."** in Build Settings window
2. Configure:
   - **Company Name**: SpaceShooterDev (or your name)
   - **Product Name**: Space Shooter
   - **Default Screen Width**: 1920
   - **Default Screen Height**: 1080
   - **Fullscreen Mode**: Fullscreen Window
   - **Resizable Window**: ✓ (checked)
   - **Run In Background**: ✗ (unchecked)

### Step 6: Build the Game

#### Quick Build Method:
1. Go to: **Tools → Space Shooter → Build Windows Executable**
2. Choose an output folder (e.g., create a `Builds` folder)
3. Wait for the build to complete
4. The executable will be at: `Builds/SpaceShooter.exe`

#### Manual Build Method:
1. Go to **File → Build Settings**
2. Click **"Build"**
3. Choose output folder
4. Name the executable `SpaceShooter.exe`
5. Click **"Save"**
6. Wait for the build process to complete

### Step 7: Run the Game

1. Navigate to your build output folder
2. Double-click **`SpaceShooter.exe`**
3. The game launches in fullscreen mode
4. Enjoy!

---

## Build Output Structure

After building, your output folder will contain:
```
Builds/
├── SpaceShooter.exe              # The game executable
├── SpaceShooter_Data/            # Game data folder (required)
│   ├── Managed/                  # .NET assemblies
│   ├── Resources/                # Game resources
│   ├── level0                    # Scene data
│   └── ...
├── MonoBleedingEdge/             # Mono runtime (if Mono build)
└── UnityCrashHandler64.exe       # Crash handler
```

**Important**: To distribute the game, you must include the ENTIRE build folder,
not just the .exe file.

---

## Distribution

To share your game:

1. Zip the entire build output folder
2. The recipient just needs to extract and run `SpaceShooter.exe`
3. No Unity installation required for players
4. Works on Windows 10/11 (64-bit)

---

## Troubleshooting

### "Scripts have compiler errors"
- Ensure you're using Unity 2021.3+ or 2022.3+
- Check that ALL script files are in the correct folders
- Go to **Edit → Preferences → External Tools** and click **Regenerate Project Files**

### "No scenes in build"
- Open your GameScene
- Go to **File → Build Settings**
- Click **"Add Open Scenes"**

### "Build fails with IL2CPP errors"
- Try switching to Mono scripting backend:
  - **Edit → Project Settings → Player → Other Settings**
  - Change **Scripting Backend** from IL2CPP to Mono

### Game runs but no objects appear
- Make sure the GameBootstrapper object exists in the scene
- Check the Console (Window → Console) for error messages
- Verify all scripts compiled without errors

### Font not displaying
- The game uses Unity's built-in Arial font
- If text appears as boxes, the built-in font may not be available
- This is rare but can happen in very old Unity versions

---

## Optional: Adding Sound Effects

The game has full audio integration but ships without audio files.
To add sounds:

1. Download free SFX from sites like:
   - https://freesound.org
   - https://opengameart.org
   - https://kenney.nl/assets (free game assets)

2. Import `.wav` or `.ogg` files into `Assets/Audio/`

3. Select the **AudioManager** GameObject in the scene
   (or find it in the Hierarchy at runtime)

4. Drag audio clips to the corresponding slots:
   - Player Shoot Clip
   - Enemy Shoot Clip
   - Player Hit Clip
   - Enemy Explosion Clip
   - Player Explosion Clip
   - Power Up Clip
   - Shield Break Clip
   - Game Over Clip
   - Background Music

The game works perfectly without sounds - the AudioManager
gracefully handles null/missing clips.

---

## System Requirements (for the built game)

- **OS**: Windows 10/11 (64-bit)
- **RAM**: 512 MB
- **GPU**: Any GPU with DirectX 11 support
- **Storage**: ~100 MB
- **Input**: Keyboard
