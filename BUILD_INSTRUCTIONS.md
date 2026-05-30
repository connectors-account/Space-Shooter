# Build Instructions — Windows Desktop .exe

## Prerequisites

| Requirement | Version | Download |
|---|---|---|
| Unity Editor | 2021.3 LTS or newer | [unity.com/download](https://unity.com/download) |
| Unity Hub | Latest | Included with Unity download |
| Windows Build Support | Included by default | Via Unity Hub → Installs → Add Modules |

> **Tip:** When installing Unity through Unity Hub, make sure
> "Windows Build Support (IL2CPP)" or "Windows Build Support (Mono)" is checked.

---

## Complete Step-by-Step Guide

### 1. Install Unity

1. Download and install **Unity Hub** from [unity.com/download](https://unity.com/download).
2. Open Unity Hub → **Installs** tab → **Install Editor**.
3. Choose **Unity 2021.3 LTS** (or any newer version).
4. In the modules selection, ensure these are checked:
   - ✅ Windows Build Support (IL2CPP) — *for best performance*
   - ✅ Windows Build Support (Mono) — *alternative, faster compile*
5. Click **Install** and wait for completion.

### 2. Create a New 2D Project

1. In Unity Hub → **Projects** tab → **New Project**.
2. Select the **2D (Built-in Render Pipeline)** template.
3. **Project Name:** `SpaceShooter`
4. **Location:** Choose where to save (e.g., `C:\UnityProjects\`).
5. Click **Create Project** and wait for Unity to open.

### 3. Import Game Files

**Option A — Copy Files (Recommended):**
1. Open your OS file manager.
2. Navigate to this project's `Assets/Scripts/` folder.
3. Copy the **entire `Scripts` folder** into your Unity project's `Assets/` directory:
   ```
   C:\UnityProjects\SpaceShooter\Assets\Scripts\
   ```
4. Switch back to Unity — it will auto-detect and compile the scripts.
5. Check the Console (Window → Console) — there should be no errors.

**Option B — Drag & Drop:**
1. In the Unity Editor, go to the **Project** panel (bottom).
2. Navigate to `Assets/`.
3. Drag the `Scripts` folder from your file manager into the Project panel.

### 4. Set Up the Scene

1. In the **Hierarchy** panel (left side):
   - Right-click → **Create Empty**.
   - Name it `Bootstrap`.
2. With `Bootstrap` selected, go to the **Inspector** panel (right side):
   - Click **Add Component**.
   - Type `SceneBootstrap` and select it.
3. Press **▶ Play** (top center) to test the game.
4. You should see the main menu. Click **PLAY** to start!

### 5. Verify Tags (Usually Automatic)

The `TagSetup.cs` script auto-creates tags on compilation. If you see tag-related errors:

1. Go to **Edit → Project Settings → Tags and Layers**.
2. Under **Tags**, add any missing:
   - `Player`
   - `Enemy`
   - `PlayerBullet`
   - `EnemyBullet`
   - `PowerUp`

### 6. Save Your Scene

1. **File → Save As** (Ctrl+Shift+S).
2. Save as `Assets/Scenes/MainScene.unity`.

### 7. Configure Build Settings

1. **File → Build Settings** (Ctrl+Shift+B).
2. Click **Add Open Scenes** — `MainScene` appears in the list.
3. Platform should already be **Windows, Mac, Linux**.
   - If not, select it and click **Switch Platform**.
4. Settings:
   - **Target Platform:** Windows
   - **Architecture:** x86_64
   - **Compression Method:** Default
   - **Copy PDB files:** Unchecked (for release builds)

### 8. Configure Player Settings

1. In Build Settings, click **Player Settings…** (bottom-left).
2. Under **Resolution and Presentation**:
   - **Fullscreen Mode:** Windowed
   - **Default Screen Width:** 600
   - **Default Screen Height:** 800
   - **Resizable Window:** ✓
3. Under **Other Settings**:
   - **Company Name:** Your name or studio
   - **Product Name:** Space Shooter
   - **Version:** 1.0
4. (Optional) Under **Icon**, assign a 256×256 PNG icon.
5. Close Player Settings.

### 9. Build the Executable

1. Back in **Build Settings**, click **Build**.
2. Create a new folder for the output (e.g., `Builds/Windows/`).
3. Click **Select Folder**.
4. Wait for the build to complete (first build takes 2-5 minutes).
5. When done, navigate to the build folder:
   ```
   Builds/Windows/
   ├── Space Shooter.exe          ← RUN THIS
   ├── Space Shooter_Data/        ← Required data
   ├── UnityPlayer.dll            ← Required runtime
   └── MonoBleedingEdge/          ← Required (Mono backend)
   ```

### 10. Run the Game!

Double-click **`Space Shooter.exe`** — the game launches in a window!

---

## Distributing Your Game

### Create a ZIP for sharing:
1. Select all files in the build output folder.
2. Right-click → **Send to → Compressed (zipped) folder**.
3. Name it `SpaceShooter_v1.0_Windows.zip`.
4. Share the ZIP — recipients just extract and run the .exe.

### Important Notes:
- Recipients **do NOT** need Unity installed.
- All files in the build folder must stay together (don't move just the .exe).
- The game requires **Windows 7 or later** and a DirectX 11 compatible GPU.
- For Windows 10/11, no additional runtime is needed.

---

## Troubleshooting

| Issue | Solution |
|---|---|
| Scripts won't compile | Ensure Unity version is 2021.3+. Check Console for errors. |
| Tags not found | Go to Edit → Project Settings → Tags and Layers → add missing tags. |
| Game doesn't start on Play | Make sure `SceneBootstrap` is on a GameObject in the scene. |
| Build fails | Ensure Windows Build Support module is installed (Unity Hub → Installs). |
| Low FPS in build | Try IL2CPP scripting backend (Player Settings → Other Settings). |
| No sound | Assign audio clips to SoundManager's sfxEntries array. |
| Player can't move | Check that the game state is "Playing" — click PLAY on the main menu first. |

---

## Optional: IL2CPP Build (Better Performance)

For a more optimized build:
1. **Player Settings → Other Settings → Scripting Backend** → change to **IL2CPP**.
2. This requires the IL2CPP module installed (Unity Hub → Installs → Add Modules).
3. Build time will be longer, but the executable runs faster.
