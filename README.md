# 🚀 Space Shooter — Unity 2D Game

A simple but complete space-shooter built with Unity. Fly your ship, blast enemies,
track your score, and try to survive as long as possible!

---

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── PlayerController.cs    — Movement (WASD/Arrows) & shooting (Space)
│   │   ├── BulletController.cs    — Bullet upward movement & collision
│   │   ├── EnemyController.cs     — Enemy downward drift + wobble
│   │   ├── EnemySpawner.cs        — Spawns enemies at intervals (ramps up)
│   │   ├── GameManager.cs         — Singleton: score, health, game state
│   │   ├── UIManager.cs           — HUD (score/health) & Game Over panel
│   │   └── Editor/
│   │       └── SceneSetupWizard.cs — One-click scene builder (Editor only)
│   ├── Scenes/
│   │   └── MainScene.unity        — (created by the wizard)
│   └── Prefabs/
│       ├── Bullet.prefab           — (created by the wizard)
│       └── Enemy.prefab            — (created by the wizard)
├── Packages/
│   └── manifest.json
├── ProjectSettings/
│   ├── ProjectSettings.asset
│   ├── TagManager.asset
│   ├── InputManager.asset
│   ├── Physics2DSettings.asset
│   ├── EditorBuildSettings.asset
│   ├── QualitySettings.asset
│   └── ProjectVersion.txt
└── README.md
```

---

## 🎮 Game Controls

| Action | Keys |
|--------|------|
| Move   | **W A S D** or **Arrow Keys** |
| Shoot  | **Spacebar** (hold for rapid fire) |

---

## 🛠️ Setup Instructions (Step-by-Step)

### Prerequisites
- **Unity Hub** installed — [download here](https://unity.com/download)
- **Unity 2022.3 LTS** (or any 2021.3+ / 2022.3+ version) installed via Unity Hub
- **Windows** machine (for building a Windows executable)

### Step 1 — Open the project
1. Launch **Unity Hub**.
2. Click **"Open" → "Add project from disk"**.
3. Navigate to the `space_shooter_game` folder and select it.
4. Unity Hub will detect the project. Click to open it.
5. If prompted about a version mismatch, click **"Continue"** — Unity will upgrade
   the project files automatically.

### Step 2 — Set up Tags (if needed)
The `TagManager.asset` pre-defines the tags, but if Unity resets them:
1. Go to **Edit → Project Settings → Tags and Layers**.
2. Make sure these tags exist:
   - `Player`
   - `Enemy`
   - `Bullet`

### Step 3 — Auto-build the scene ⭐
1. In the Unity menu bar, click **Space Shooter → Setup Scene (Auto-Build Everything)**.
2. The wizard will create:
   - Main Camera (orthographic, dark background)
   - Player ship (cyan rectangle) with `PlayerController`
   - Bullet prefab (yellow) in `Assets/Prefabs/`
   - Enemy prefab (red) in `Assets/Prefabs/`
   - GameManager & EnemySpawner objects
   - Full UI Canvas with Score, Health, and Game Over panel
   - Scene saved as `Assets/Scenes/MainScene.unity`
3. A confirmation dialog will appear when done.

### Step 4 — Test in the Editor
1. Press **▶ Play** in Unity.
2. Move with WASD, shoot with Space.
3. Red enemies spawn from the top. Shoot them for 100 points each.
4. If an enemy hits you, you lose 1 HP (starting HP = 5).
5. When HP reaches 0, the Game Over screen appears.
6. Click **RESTART** to play again.

---

## 🏗️ Building a Windows Executable

### Step 1 — Set the build target
1. Go to **File → Build Settings**.
2. Select **"PC, Mac & Linux Standalone"** in the platform list.
3. Set **Target Platform** = **Windows**.
4. Set **Architecture** = **x86_64** (recommended).
5. Click **"Switch Platform"** if it's not already selected.

### Step 2 — Add the scene
1. In the Build Settings window, click **"Add Open Scenes"**.
2. You should see `Assets/Scenes/MainScene.unity` with index 0.

### Step 3 — Configure Player Settings (optional)
1. Click **"Player Settings..."** in the Build Settings window.
2. Set the **Product Name** (e.g., "Space Shooter").
3. Set the **Default Screen Width/Height** (e.g., 1024 × 768).
4. Set **Fullscreen Mode** to **Windowed** for easier testing.

### Step 4 — Build
1. Click **"Build"** (or **"Build and Run"** to launch immediately).
2. Choose an output folder (e.g., `Builds/Windows/`).
3. Unity will compile and produce:
   - `Space Shooter.exe` — the game executable
   - `Space Shooter_Data/` — required data folder
   - `UnityPlayer.dll` — required runtime DLL
4. **To distribute**: zip the entire output folder.

---

## 🔧 Customization

| Parameter | Where | Default |
|-----------|-------|---------|
| Player speed | PlayerController → `moveSpeed` | 8 |
| Fire rate | PlayerController → `fireRate` | 0.25s |
| Bullet speed | BulletController → `speed` | 12 |
| Enemy speed | EnemyController → `speed` | 4 |
| Spawn interval | EnemySpawner → `spawnInterval` | 1.5s |
| Min spawn interval | EnemySpawner → `minSpawnInterval` | 0.4s |
| Player max health | GameManager → `maxHealth` | 5 |
| Points per kill | BulletController `OnTriggerEnter2D` | 100 |

All values are exposed in the Unity Inspector — just select the GameObject and tweak.

---

## 🎨 Improving Visuals (Optional)

- Replace the colored rectangles with proper sprite art (just swap the `SpriteRenderer.sprite`).
- Add a scrolling star-field background.
- Add particle effects for explosions.
- Add sound effects using Unity's `AudioSource`.

---

## ❓ Troubleshooting

| Problem | Solution |
|---------|----------|
| "Tag not found" error | Add `Player`, `Enemy`, `Bullet` tags in Edit → Project Settings → Tags and Layers |
| Bullets don't hit enemies | Make sure both have **Collider2D** with **Is Trigger = true** and a **Rigidbody2D** |
| No enemies spawning | Check EnemySpawner has the Enemy prefab assigned |
| UI not showing | Ensure Canvas exists with a child UIManager component |
| Scene setup menu missing | Make sure `SceneSetupWizard.cs` is in `Assets/Scripts/Editor/` |

---

**Enjoy the game! 🎮**
