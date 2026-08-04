# Space Shooter – Unity Setup & Windows Build Guide

## Prerequisites
| Tool | Version |
|------|---------|
| Unity Hub | Latest |
| Unity Editor | **2022.3 LTS** (or newer) |
| TextMeshPro | Installed via Package Manager |
| Build Target | **Windows, Mac, Linux (Standalone)** |

---

## 1. Create the Unity Project

1. Open **Unity Hub → New Project**.
2. Select template **2D (Core)**.
3. Name it `SpaceShooter`, choose a folder, click **Create**.

---

## 2. Copy Scripts

Place all `.cs` files from `Assets/Scripts/` into  
`<your-project>/Assets/Scripts/` (create the folder if missing).

---

## 3. Create Sprites (Quick-Start with Primitives)

Unity's built-in shapes are enough to get the game running. Swap them later
for pixel-art sprites.

| Object | How to make | Colour |
|--------|-------------|--------|
| Player ship | `GameObject → 2D Object → Sprites → Triangle` | Cyan `#00FFFF` |
| Player bullet | `GameObject → 2D Object → Sprites → Circle` (scale 0.15 × 0.4) | Yellow `#FFFF00` |
| Straight enemy | `GameObject → 2D Object → Sprites → Quad` | Red `#FF4444` |
| Zigzag enemy | `GameObject → 2D Object → Sprites → Diamond` (rotated Quad) | Magenta `#FF00FF` |
| Shooter enemy | `GameObject → 2D Object → Sprites → Pentagon` | Orange `#FF8800` |
| Enemy bullet | `GameObject → 2D Object → Sprites → Circle` (scale 0.15 × 0.4) | Red `#FF2222` |
| Background | `GameObject → 2D Object → Sprites → Square` (scale 20 × 14) | Dark Blue `#050520` |

> Tip: assign colours by selecting the SpriteRenderer in the Inspector and
> changing its **Color** field.

---

## 4. Create Prefabs

For each object listed above:
1. Create the sprite in the Hierarchy.
2. Add required components (see table below).
3. Drag it from the Hierarchy into `Assets/Prefabs/` → it becomes a prefab.
4. Delete the original from the Hierarchy.

### Component Reference

#### Player
| Component | Settings |
|-----------|---------|
| `SpriteRenderer` | Sprite = Triangle, Color = Cyan |
| `PolygonCollider2D` | Is Trigger = **true** |
| `Rigidbody2D` | Body Type = Kinematic, Gravity = 0 |
| `PlayerController` | bulletPrefab = PlayerBullet prefab, firePoint = (child empty GO above ship) |
- Tag: **Player**
- Layer: **Player**

#### PlayerBullet
| Component | Settings |
|-----------|---------|
| `SpriteRenderer` | Sprite = Circle, Color = Yellow |
| `CircleCollider2D` | Is Trigger = **true** |
| `Rigidbody2D` | Body Type = Kinematic, Gravity = 0 |
| `Bullet` | speed = 14, damage = 1 |
- Tag: **PlayerBullet**

#### StraightEnemy / ZigzagEnemy / ShooterEnemy
| Component | Settings |
|-----------|---------|
| `SpriteRenderer` | (see colour table above) |
| `PolygonCollider2D` | Is Trigger = **true** |
| `Rigidbody2D` | Body Type = Kinematic, Gravity = 0 |
| `Enemy` | enemyType = Straight / Zigzag / Shooter |
- ShooterEnemy only: assign **enemyBulletPrefab** to the EnemyBullet prefab.
- Tag: **Enemy**

#### EnemyBullet
| Component | Settings |
|-----------|---------|
| `SpriteRenderer` | Sprite = Circle, Color = Red |
| `CircleCollider2D` | Is Trigger = **true** |
| `Rigidbody2D` | Body Type = Kinematic, Gravity = 0 |
| `EnemyBullet` | speed = 6 |
- Tag: **EnemyBullet**

### Physics 2D Layer Matrix (Project Settings → Physics 2D)
Enable collisions only for these pairs (all others OFF):

| Layer A | Layer B |
|---------|---------|
| Player | Enemy |
| Player | EnemyBullet |
| PlayerBullet | Enemy |

---

## 5. Build the Three Scenes

### Scene 1 – MainMenu
1. `File → New Scene → Basic (Built-in)` → save as `MainMenu`.
2. Add **Camera** (already present). Background colour = `#050520`.
3. Add `GameObject → UI → Canvas`:
   - Add a `TextMeshPro - Text (UI)` → text = **"SPACE SHOOTER"**, large font.
   - Add two **Buttons** labelled **PLAY** and **QUIT**.
4. Create an empty `GameObject` named `MenuController`, attach **MainMenu.cs**.
5. Wire buttons:
   - PLAY button → OnClick → MenuController → `MainMenu.OnPlayClicked()`
   - QUIT button → OnClick → MenuController → `MainMenu.OnQuitClicked()`

### Scene 2 – Game
1. `File → New Scene → Basic (Built-in)` → save as `Game`.

#### Hierarchy structure
```
Game (scene root)
├── Main Camera           (orthographic, size = 5.4, bg color = #050520)
├── Background_A          (SpriteRenderer + ParallaxBackground, scrollSpeed = 2)
├── Background_B          (identical, positioned one tile ABOVE Background_A)
├── Player                (Player prefab, start pos = 0, -3.5, 0)
├── Spawner               (empty GO + EnemySpawner.cs; assign 3 enemy prefabs)
├── GameManagerHolder     (empty GO + GameManager.cs)
└── Canvas                (UI Canvas + UIManager.cs)
    ├── ScoreText         (TextMeshProUGUI, top-left)
    ├── HealthText        (TextMeshProUGUI, top-right)
    └── GameOverPanel     (Panel, disabled by default)
        └── GameOverText  (TextMeshProUGUI → "GAME OVER")
```

#### UIManager wiring
Select the Canvas GameObject → Inspector → UIManager:
- `scoreText` → ScoreText
- `healthText` → HealthText
- `gameOverPanel` → GameOverPanel

### Scene 3 – GameOver
1. `File → New Scene → Basic (Built-in)` → save as `GameOver`.
2. Add a Canvas with:
   - `finalScoreText` (TextMeshProUGUI)
   - `highScoreText`  (TextMeshProUGUI)
   - **PLAY AGAIN** button → `GameOverScreen.OnPlayAgainClicked()`
   - **MAIN MENU** button → `GameOverScreen.OnMainMenuClicked()`
3. Attach **GameOverScreen.cs** to an empty GameObject, wire the two text fields.

---

## 6. Add Scenes to Build Settings

`File → Build Settings`:
1. Click **Add Open Scenes** for each of the three scenes.
2. Order: `MainMenu (0)`, `Game (1)`, `GameOver (2)`.

Confirm scene names match the strings in `SceneManager.LoadScene("…")` calls.

---

## 7. Player Input (no extra package needed)

The game uses Unity's legacy **Input Manager** (built-in):
- Movement: `Horizontal` / `Vertical` axes (WASD + Arrow keys already mapped).
- Fire: `KeyCode.Space` – held to auto-fire.

---

## 8. Build Windows Executable

1. `File → Build Settings`.
2. Platform = **Windows, Mac, Linux Standalone**.
3. Architecture = **x86_64**.
4. Click **Player Settings** → set:
   - Product Name = `SpaceShooter`
   - Default Screen Width = `1280`, Height = `720`
   - Fullscreen Mode = `Windowed` (or Fullscreen Exclusive)
5. Back in Build Settings → **Build**.
6. Choose an output folder (e.g. `Build/`).
7. Unity produces `SpaceShooter.exe` + `SpaceShooter_Data/` + `UnityPlayer.dll`.  
   **Distribute the entire folder** – the `.exe` alone won't run.

---

## 9. Controls Summary

| Key | Action |
|-----|--------|
| WASD / Arrow Keys | Move ship |
| Space (hold) | Fire bullets |
| Alt+F4 / Quit button | Exit |

---

## 10. Difficulty Scaling Reference

| Time elapsed | Spawn interval | Enemy mix |
|---|---|---|
| 0 – 22 s | ~1.6 s | 80% Straight, 20% Zigzag |
| 22 – 50 s | ~1.0 s | 45% Straight, 35% Zigzag, 20% Shooter |
| 50 s+ | ~0.35 s | 25% Straight, 40% Zigzag, 35% Shooter |

---

## Script File Map

```
Assets/
└── Scripts/
    ├── GameManager.cs        ← singleton, score, health, scene flow
    ├── PlayerController.cs   ← movement, shooting, invincibility flash
    ├── Bullet.cs             ← player bullet (upward, destroys enemies)
    ├── EnemyBullet.cs        ← enemy bullet (downward, damages player)
    ├── Enemy.cs              ← 3 enemy types + collision + scoring
    ├── EnemySpawner.cs       ← time-based spawning with difficulty ramp
    ├── ParallaxBackground.cs ← infinite looping starfield scroll
    ├── UIManager.cs          ← HUD: score, health hearts, game-over panel
    ├── MainMenu.cs           ← main menu button handlers
    └── GameOverScreen.cs     ← game-over screen + high score (PlayerPrefs)
```
