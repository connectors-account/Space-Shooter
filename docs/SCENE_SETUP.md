# Manual Scene Setup Guide

This guide explains how to set up the game scene manually in the Unity Editor, as an alternative to using the automatic `SceneBootstrap` script.

---

## Prerequisites

1. Open Unity 2021.3+ with the project loaded
2. Import **TextMeshPro Essentials** when prompted
3. Configure **Tags** in Edit → Project Settings → Tags and Layers:
   - Tag 0: `PlayerBullet`
   - Tag 1: `EnemyBullet`
   - Tag 2: `Enemy`
   - Tag 3: `PowerUp`
   - The built-in `Player` tag should already exist

---

## Step 1: Create a New Scene

1. File → New Scene → Basic (Built-in)
2. Save as `Assets/Scenes/MainScene.unity`

---

## Step 2: Camera Setup

1. Select the **Main Camera**
2. Set Background Color: `(5, 5, 20, 255)` — very dark blue
3. Set Projection: **Orthographic**
4. Set Orthographic Size: **5**
5. Position: `(0, 0, -10)`

---

## Step 3: Create Manager GameObjects

Create empty GameObjects for each manager. Attach the corresponding script:

| GameObject Name | Script to Attach |
|----------------|-----------------|
| `GameManager` | `Scripts/Managers/GameManager.cs` |
| `AudioManager` | `Scripts/Managers/AudioManager.cs` |
| `WaveSpawner` | `Scripts/Managers/WaveSpawner.cs` |
| `ObjectPoolManager` | `Scripts/Managers/ObjectPoolManager.cs` |
| `ExplosionManager` | `Scripts/Effects/ExplosionManager.cs` |
| `PowerUpSpawner` | `Scripts/PowerUps/PowerUpSpawner.cs` |
| `Starfield` | `Scripts/Effects/StarfieldGenerator.cs` |

---

## Step 4: Create the Player

1. Create: GameObject → 2D Object → Sprite → Square
2. Rename to `Player`
3. Set Tag to `Player`
4. Position: `(0, -3.5, 0)`
5. Add Components:
   - `Rigidbody2D` (Gravity Scale = 0, Freeze Rotation = true)
   - `BoxCollider2D` (Is Trigger = true, Size = 0.6 × 0.8)
   - `Scripts/Player/PlayerController.cs`
6. Set SpriteRenderer color to cyan `(50, 200, 255)`
7. Create child GameObject `FirePoint` at local position `(0, 0.6, 0)`
8. Create child GameObject `ShieldVisual`:
   - Add SpriteRenderer with a circle sprite, color cyan with alpha 0.3
   - Set active = false
9. Wire references in PlayerController inspector:
   - Fire Point → `FirePoint` transform
   - Shield Visual → `ShieldVisual` GameObject
   - Sprite Renderer → Player's SpriteRenderer

---

## Step 5: Create Bullet Prefabs

### Player Bullet Prefab
1. Create: GameObject → 2D Object → Sprite → Square
2. Name: `PlayerBulletPrefab`
3. Tag: `PlayerBullet`
4. Scale: `(0.3, 0.6, 1)`
5. Color: Cyan
6. Add: `Rigidbody2D` (Gravity = 0), `BoxCollider2D` (Trigger), `Weapons/Bullet.cs`
7. Drag to `Assets/Prefabs/`, delete from scene

### Enemy Bullet Prefab
1. Same as above but:
   - Name: `EnemyBulletPrefab`
   - Tag: `EnemyBullet`
   - Color: Red
2. Save as prefab

---

## Step 6: Create Enemy Prefabs

### Basic Enemy
1. Create sprite, name `BasicEnemyPrefab`, tag `Enemy`
2. Scale: `(0.4, 0.4, 1)`, Color: Red
3. Flip Y on SpriteRenderer
4. Add: `Rigidbody2D` (Gravity=0), `BoxCollider2D` (Trigger), `Enemies/BasicEnemy.cs`
5. Save as prefab

### Fast Enemy
1. Same but: Name `FastEnemyPrefab`, Color: Magenta, Scale 0.3
2. Use `Enemies/FastEnemy.cs`
3. Save as prefab

### Tank Enemy
1. Same but: Name `TankEnemyPrefab`, Color: Orange, Scale 0.6
2. Use `Enemies/TankEnemy.cs`
3. Save as prefab

---

## Step 7: Create Power-Up Prefab

1. Create sprite, name `PowerUpPrefab`, tag `PowerUp`
2. Scale `(0.5, 0.5, 1)`, Color: Green
3. Add `BoxCollider2D` (Trigger), `PowerUps/PowerUpItem.cs`
4. Save as prefab

---

## Step 8: Configure Object Pools

1. Select the `ObjectPoolManager` GameObject
2. In the Inspector, expand the **Pools** list
3. Add 6 entries:

| Tag | Prefab | Initial Size |
|-----|--------|-------------|
| `PlayerBullet` | PlayerBulletPrefab | 30 |
| `EnemyBullet` | EnemyBulletPrefab | 30 |
| `BasicEnemy` | BasicEnemyPrefab | 15 |
| `FastEnemy` | FastEnemyPrefab | 10 |
| `TankEnemy` | TankEnemyPrefab | 5 |
| `PowerUp` | PowerUpPrefab | 5 |

---

## Step 9: Create UI Canvas

1. GameObject → UI → Canvas
2. Set Canvas Scaler:
   - UI Scale Mode: Scale with Screen Size
   - Reference Resolution: 1920 × 1080
3. Add **EventSystem** if not auto-created

### Main Menu Panel
- Create Panel child, name `MainMenuPanel`
- Background: Black, alpha 85%
- Add children:
  - TextMeshPro: "SPACE SHOOTER" (48pt, cyan, centered, Y=120)
  - TextMeshPro: High Score text (24pt, yellow, Y=50)
  - Button: "START GAME" (green bg, Y=-30)
  - Button: "QUIT" (red bg, Y=-110)
- Create empty `MainMenuController`, attach `UI/MainMenuUI.cs`, wire references

### HUD Panel
- Create Panel child, name `HudPanel`, transparent background
- Add TextMeshPro children:
  - `ScoreText`: "SCORE: 0" (28pt, white, top-left)
  - `WaveText`: "WAVE 1" (28pt, yellow, top-center)
  - `HealthText`: "LIVES: 5/5" (28pt, green, top-right)
- Create `HudController`, attach `UI/HudUI.cs`, wire references

### Pause Panel
- Create Panel, name `PausePanel`, dark semi-transparent
- Add: "PAUSED" title, Resume button, Main Menu button
- Create `PauseMenuController`, attach `UI/PauseMenuUI.cs`

### Game Over Panel
- Create Panel, name `GameOverPanel`, dark background
- Add: "GAME OVER" (red), Final Score, High Score, Play Again button, Main Menu button
- Create `GameOverController`, attach `UI/GameOverUI.cs`

---

## Step 10: Save and Test

1. Save the scene: Ctrl+S
2. Press Play ▶️
3. The main menu should appear
4. Click "START GAME" to begin
5. Use Arrow Keys / WASD to move, Space to shoot
6. Press Escape to pause

---

## Step 11: Build for Windows

1. File → Build Settings
2. Add Open Scenes
3. Target: PC, Mac & Linux Standalone
4. Target Platform: Windows
5. Architecture: x86_64
6. Click Build
7. Select output folder
8. Distribute the entire build folder
