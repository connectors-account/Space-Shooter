# Space Shooter — Complete Build Instructions for Windows .exe

This document provides step-by-step instructions to set up, configure, and
build the Space Shooter game into a standalone Windows desktop executable.

---

## Prerequisites

| Tool | Version | Download |
|------|---------|----------|
| Unity Hub | Latest | https://unity.com/download |
| Unity Editor | **2022.3 LTS** or **6000.x (Unity 6)** | Install via Unity Hub |
| Windows Build Support | IL2CPP + Mono | Install via Unity Hub → Installs → Add Modules |
| Visual Studio 2022 or Rider | (optional, for editing) | |

> **Important:** When installing Unity, ensure you check the box for
> **"Windows Build Support (IL2CPP)"** and **"Windows Build Support (Mono)"**
> under the platform modules.

---

## Step 1 — Open the Project in Unity

1. Launch **Unity Hub**
2. Click **"Open"** → browse to the `SpaceShooterUnity` folder
3. Select the folder and click **"Open"**
4. Unity will import assets and compile scripts (first time takes 1-3 minutes)
5. If prompted about a Unity version, click **"Continue"** to upgrade

---

## Step 2 — Verify Project Settings

1. Go to **Edit → Project Settings → Player**
2. Confirm:
   - **Company Name:** SpaceShooterStudio
   - **Product Name:** Space Shooter
   - **Default Screen Width:** 1080
   - **Default Screen Height:** 1920
   - **Fullscreen Mode:** Windowed (or Fullscreen Window)
   - **Resizable Window:** ✓ (checked)
   - **Run In Background:** ✓ (checked)

3. Go to **Edit → Project Settings → Player → Other Settings**:
   - **Scripting Backend:** IL2CPP (recommended for release) or Mono (faster builds)
   - **Api Compatibility Level:** .NET Standard 2.1 or .NET Framework
   - **Target Architecture:** x86_64

4. Go to **Edit → Project Settings → Physics 2D**:
   - Verify **Gravity** = (0, 0) — our scripts handle all movement
   - Configure layer collision matrix per the Scene Setup guide

---

## Step 3 — Create Sprites (Minimal Quick-Start)

If you don't have art assets yet, create placeholder sprites:

1. In the Project window, right-click `Assets/Sprites/`
2. **Create → Sprites → Square** — name it "WhiteSquare"
3. Use this single sprite for all prefabs — the scripts color-tint everything automatically

Or download the free **Kenney Space Shooter Redux** pack from https://kenney.nl/assets/space-shooter-redux

---

## Step 4 — Create Prefabs

Follow the detailed guide in `Assets/Prefabs/README_PREFABS.md`.

Summary of required prefabs:
- `Player.prefab` — PlayerController + HealthSystem + Rigidbody2D + Collider2D
- `PlayerBullet.prefab` — BulletController + Rigidbody2D + Collider2D
- `EnemyBullet.prefab` — BulletController + Rigidbody2D + Collider2D
- `Enemy_Basic.prefab` — EnemyController + HealthSystem + Rigidbody2D + Collider2D
- `Enemy_Zigzag.prefab` — same as above, different pattern
- `Enemy_Dive.prefab` — same as above, different pattern
- `PowerUp_Weapon.prefab` — PowerUpController
- `PowerUp_Shield.prefab` — PowerUpController
- `PowerUp_Health.prefab` — PowerUpController
- `PowerUp_Rapid.prefab` — PowerUpController
- `PowerUp_Score.prefab` — PowerUpController

---

## Step 5 — Set Up the Scene

Follow the detailed guide in `Assets/Scenes/README_SCENE_SETUP.md`.

Quick checklist:
- [ ] Camera: Orthographic, Size 6, Background dark
- [ ] GameManager object with `GameManager.cs`
- [ ] UIManager object with `UIManager.cs` and full Canvas UI
- [ ] AudioManager object with `AudioManager.cs`
- [ ] GameInitializer object with `GameInitializer.cs` — all references wired
- [ ] EnemySpawner object with `EnemySpawner.cs` — enemy prefabs array filled
- [ ] PlayerSpawnPoint empty at (0, -4, 0)
- [ ] All tags created: Player, Enemy, PlayerBullet, EnemyBullet, PowerUp
- [ ] All layers created: Player, PlayerBullet, EnemyBullet, Enemy, PowerUp

---

## Step 6 — Test in Editor

1. Press **Play** (▶) in the Unity Editor
2. Verify:
   - Main Menu appears with Start/Quit buttons
   - Clicking Start spawns the player and begins enemy waves
   - WASD/Arrow keys move the ship
   - Space/Left-click fires bullets
   - Enemies move and shoot back
   - Score updates when enemies are destroyed
   - Power-ups drop and can be collected
   - ESC pauses the game
   - Game Over screen appears when lives run out
   - Restart and Main Menu buttons work

---

## Step 7 — Build the Windows Executable

### Method A: Unity Build Dialog (Recommended)

1. Go to **File → Build Settings** (Ctrl+Shift+B)

2. **Platform** panel on the left:
   - Select **"Windows, Mac, Linux"** (or "PC, Mac & Linux Standalone")
   - If not already selected, click **"Switch Platform"** (may take a minute)

3. **Target Platform:** Windows
   **Architecture:** x86_64

4. **Scenes In Build:**
   - Click **"Add Open Scenes"** to add `GameScene`
   - Ensure it shows as index 0

5. Click **"Player Settings..."** and verify:
   - **Scripting Backend:** IL2CPP (better performance) or Mono (faster build)
   - **Api Compatibility:** .NET Standard 2.1
   - **IL2CPP Code Generation:** Faster runtime (for release)

6. Click **"Build"**
   - Choose/create folder: `Builds/Windows/`
   - Name the exe: `SpaceShooter.exe`
   - Wait for build to complete (2-10 minutes depending on backend)

7. Navigate to `Builds/Windows/` — you'll find:
   ```
   SpaceShooter.exe            ← The executable
   SpaceShooter_Data/          ← Game data folder (REQUIRED)
   UnityPlayer.dll             ← Unity runtime (REQUIRED)
   UnityCrashHandler64.exe     ← Crash reporter
   MonoBleedingEdge/           ← (Mono backend only)
   ```

### Method B: Command-Line Build (CI/Automation)

```bash
# Set your Unity Editor path
UNITY_PATH="C:/Program Files/Unity/Hub/Editor/2022.3.XXf1/Editor/Unity.exe"

# Build from command line
"$UNITY_PATH" \
  -quit \
  -batchmode \
  -nographics \
  -projectPath "C:/path/to/SpaceShooterUnity" \
  -buildTarget Win64 \
  -buildWindows64Player "C:/path/to/Builds/Windows/SpaceShooter.exe" \
  -logFile "build.log"
```

### Method C: Build Script (automated via C# editor script)

Create `Assets/Editor/BuildScript.cs`:

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildScript
{
    [MenuItem("Build/Build Windows x64")]
    public static void BuildWindows()
    {
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/GameScene.unity" },
            locationPathName = "Builds/Windows/SpaceShooter.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
            Debug.Log($"Build succeeded: {summary.totalSize} bytes, {summary.totalTime}");
        else
            Debug.LogError($"Build failed: {summary.result}");
    }
}
#endif
```

Then run via **Build → Build Windows x64** menu item.

---

## Step 8 — Distribute the Build

### Zip for Distribution

The entire build folder must be distributed together:

```
SpaceShooter_Windows.zip
├── SpaceShooter.exe
├── SpaceShooter_Data/
├── UnityPlayer.dll
├── UnityCrashHandler64.exe
└── MonoBleedingEdge/     (if using Mono backend)
```

> ⚠️ **Do NOT** distribute just the `.exe` — it requires the `_Data` folder
> and DLLs to run.

### Optional: Create Installer with Inno Setup

1. Download Inno Setup from https://jrsoftware.org/isinfo.php
2. Run the Inno Setup Script Wizard
3. Point it at your build folder
4. It creates a single `SpaceShooter_Setup.exe` installer

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "Build Support not installed" | Unity Hub → Installs → gear icon → Add Modules → Windows Build Support |
| Scripts have compile errors | Check Console (Ctrl+Shift+C), fix red errors first |
| Game runs but no enemies spawn | Ensure EnemySpawner has prefabs in the array |
| Player doesn't move | Check Rigidbody2D Gravity Scale = 0, Time.timeScale = 1 |
| Bullets pass through enemies | Ensure both have Collider2D with Is Trigger = ✓ and correct tags |
| UI not showing | Ensure Canvas is Screen Space - Overlay, panels are active |
| Build crashes on launch | Try Mono backend instead of IL2CPP; check `output_log.txt` in `_Data/` |
| IL2CPP build fails | Install Visual Studio 2022 with "Desktop development with C++" workload |
| Black screen on launch | Add scene to Build Settings (File → Build Settings → Add Open Scenes) |

---

## System Requirements for the Built Game

| Requirement | Minimum |
|-------------|---------|
| OS | Windows 10 (64-bit) |
| CPU | Any x86_64 processor |
| RAM | 2 GB |
| GPU | DirectX 11 capable |
| Storage | ~100 MB |

---

## Controls Reference

| Action | Input |
|--------|-------|
| Move | WASD or Arrow Keys |
| Shoot | Space or Left Mouse Button |
| Pause | Escape |

---

## Project Structure Overview

```
SpaceShooterUnity/
├── Assets/
│   ├── Scripts/
│   │   ├── GameManager.cs        — Game state, scoring, lives
│   │   ├── GameInitializer.cs    — Scene bootstrap and wiring
│   │   ├── PlayerController.cs   — Movement, shooting, power-ups
│   │   ├── EnemyController.cs    — AI movement patterns, shooting
│   │   ├── BulletController.cs   — Projectile physics, damage
│   │   ├── EnemySpawner.cs       — Wave-based enemy spawning
│   │   ├── PowerUpController.cs  — Collectible effects
│   │   ├── HealthSystem.cs       — Reusable HP component
│   │   ├── UIManager.cs          — HUD, menus, panels
│   │   ├── AudioManager.cs       — Music and SFX
│   │   └── BackgroundScroller.cs — Scrolling starfield
│   ├── Scenes/
│   ├── Prefabs/
│   ├── Sprites/
│   ├── Audio/
│   ├── Materials/
│   └── UI/
├── ProjectSettings/
├── Packages/
└── BUILD_INSTRUCTIONS.md          ← You are here
```
