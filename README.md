# SpaceShooterGame (Unity, Windows Desktop)

A complete 2D space-shooter starter project structure with fully implemented gameplay scripts.

## Unity Version
- Recommended: **Unity 2021.3 LTS or newer**
- Rendering: Built-in pipeline (simple 2D setup)
- Input: **Legacy Input Manager** (not the new Input System package)

---

## Project Structure

```text
SpaceShooterGame/
├── Assets/
│   ├── Prefabs/
│   ├── Scenes/
│   └── Scripts/
│       ├── BackgroundScroller.cs
│       ├── Bullet.cs
│       ├── EnemyController.cs
│       ├── EnemySpawner.cs
│       ├── GameManager.cs
│       ├── MenuManager.cs
│       ├── PlayerController.cs
│       ├── PlayerHealth.cs
│       ├── PowerUp.cs
│       └── UIManager.cs
└── README.md
```

---

## Scripts Overview

1. **PlayerController.cs**
   - WASD/Arrow movement (Horizontal/Vertical axes)
   - Shooting with **Left Ctrl / Mouse0 / Space**
   - Movement clamped to screen bounds

2. **PlayerHealth.cs**
   - Health tracking, damage/heal, death handling
   - Notifies `GameManager` on death

3. **EnemyController.cs**
   - Moves enemies downward
   - Takes bullet damage
   - Damages player on contact

4. **EnemySpawner.cs**
   - Coroutine-based wave spawning
   - Spawns random X positions from top

5. **Bullet.cs**
   - Bullet forward movement
   - Enemy collision handling
   - Auto-destroy on timeout/boundary

6. **PowerUp.cs**
   - Falling collectible
   - Restores player health on pickup

7. **GameManager.cs**
   - Score tracking
   - Game-over state
   - Restart and menu flow

8. **UIManager.cs**
   - Score/health UI updates
   - Game-over panel control

9. **BackgroundScroller.cs**
   - Texture-offset scrolling for parallax-like effect

10. **MenuManager.cs**
    - Main menu Start and Quit actions

---

## Step-by-Step Unity Setup

## 1) Create/Open Project
1. Open **Unity Hub**.
2. Create a **2D Core** project (or open an existing 2D project).
3. Set project location to:  
   `.../SpaceShooterGame`  
   (this folder already contains the scripts).

## 2) Create Scenes
Create two scenes in `Assets/Scenes`:
- `MainMenu.unity`
- `GameScene.unity`

Add both scenes to **File > Build Settings > Scenes In Build** in this order:
1. MainMenu
2. GameScene

## 3) Configure Tags
Add tags in **Edit > Project Settings > Tags and Layers**:
- `Player`
- `Enemy`
- `Boundary`

## 4) Build MainMenu Scene
1. Create Canvas (UI):
   - Title text: "Space Shooter"
   - Start button
   - Quit button
2. Create empty object `MenuManager` and attach `MenuManager.cs`.
3. In `MenuManager` inspector:
   - `gameplaySceneName = GameScene`
4. Hook buttons:
   - Start button OnClick -> `MenuManager.StartGame`
   - Quit button OnClick -> `MenuManager.QuitGame`

## 5) Build GameScene Core Objects

### A. Managers
1. Create empty object `GameManager` and attach `GameManager.cs`.
2. Create empty object `UIManager` and attach `UIManager.cs`.

### B. Player
1. Create player ship sprite object `Player`.
2. Add components:
   - `Rigidbody2D` (Body Type: Kinematic recommended)
   - `Collider2D` (e.g., CircleCollider2D, set Is Trigger = true)
   - `PlayerController.cs`
   - `PlayerHealth.cs`
3. Tag as `Player`.
4. Create child empty object `FirePoint` near ship nose.
5. Assign `FirePoint` and `Bullet` prefab to `PlayerController`.

### C. Bullet Prefab
1. Create sprite `Bullet`.
2. Add:
   - `Collider2D` (Is Trigger = true)
   - `Bullet.cs`
3. Make prefab and save in `Assets/Prefabs/Bullet.prefab`.

### D. Enemy Prefab
1. Create sprite `Enemy`.
2. Add:
   - `Collider2D` (Is Trigger = true)
   - `EnemyController.cs`
3. Tag as `Enemy`.
4. Save as `Assets/Prefabs/Enemy.prefab`.

### E. Enemy Spawner
1. Create empty object `EnemySpawner`.
2. Attach `EnemySpawner.cs`.
3. Assign `Enemy` prefab.
4. Tune wave settings in inspector.

### F. Power-Up Prefab (Health)
1. Create sprite `HealthPowerUp`.
2. Add `Collider2D` (Is Trigger = true).
3. Attach `PowerUp.cs`.
4. Save as prefab in `Assets/Prefabs/HealthPowerUp.prefab`.
5. (Optional) You can instantiate from your own custom spawner later.

### G. Boundaries (Cleanup)
Create off-screen trigger colliders tagged `Boundary`:
- Top boundary (destroy bullets)
- Bottom boundary (destroy enemies/powerups)

### H. Background
1. Create background quad/sprite.
2. Attach `BackgroundScroller.cs`.
3. Use a tiling texture/material for visible scrolling effect.

## 6) UI Setup (GameScene)
1. Create Canvas and EventSystem.
2. HUD elements:
   - Text: `ScoreText`
   - Text: `HealthText`
   - Slider: `HealthSlider`
3. Game Over panel:
   - Panel object `GameOverPanel` (inactive by default)
   - Text `FinalScoreText`
   - Button `Restart` -> `UIManager.RestartGameFromButton`
   - Button `Main Menu` -> `UIManager.BackToMenu`
4. Assign all references in `UIManager` inspector.

---

## Input System Setup (Legacy Input Manager)

Use legacy input (default in older templates):

1. Open **Edit > Project Settings > Player > Other Settings**.
2. Set **Active Input Handling** to:
   - `Input Manager (Old)`
   - or `Both` (if you also keep new input package)
3. In **Edit > Project Settings > Input Manager**, ensure axes/buttons exist:
   - `Horizontal`
   - `Vertical`
   - `Fire1`

Default mappings already support keyboard + mouse in most Unity templates.

---

## How to Play
- Move: **WASD** or **Arrow Keys**
- Shoot: **Left Ctrl**, **Mouse Left**, or **Space**
- Restart after Game Over: **R**

---

## Build for Windows

1. Open **File > Build Settings**.
2. Platform: **PC, Mac & Linux Standalone**.
3. Target Platform: **Windows**.
4. Architecture: **x86_64**.
5. Click **Switch Platform** (if needed).
6. Click **Build**.
7. Choose output folder, e.g. `Builds/Windows/`.
8. Run generated `.exe` from output folder.

---

## Recommended Physics Settings
- Use **2D colliders** with `Is Trigger` enabled for hit detection used by scripts.
- Ensure collision matrix allows interactions:
  - Bullet ↔ Enemy
  - Enemy ↔ Player
  - Bullet/Enemy/PowerUp ↔ Boundary

---

## Notes
- This project is intentionally simple and script-driven for quick setup.
- You can extend it with:
  - Enemy shooting
  - Explosion VFX/SFX
  - Better wave progression and difficulty scaling
  - Power-up spawner
  - Persistent high scores
