# 🚀 Space Shooter — Unity C# (Windows Desktop)

A complete, fully-playable 2D space shooter with:
- Infinite wave progression with boss waves every 5th wave
- 5 distinct enemy movement patterns
- 6 bullet patterns (single, spread 3/5, aimed, circle-8, burst)
- 5 power-up types (Rapid Fire, Triple Shot, Shield, Heal, Speed Boost)
- Kill-streak score multiplier (x1 → x4)
- Procedurally generated sprites (no art files needed)
- Procedurally synthesised audio (no audio files needed)
- Parallax star-field background
- Camera shake on explosions
- Pause menu, Game Over screen, High Score (PlayerPrefs)

---

## ✅ Requirements

| Tool | Version |
|------|---------|
| Unity Hub | Latest |
| Unity Editor | **2022.3 LTS** (recommended) |
| TextMeshPro | Bundled with Unity – import via Package Manager |
| Build Target | **PC, Mac & Linux Standalone → Windows x86_64** |

---

## 📁 Project Structure

```
SpaceShooter/
└── Assets/
    └── Scripts/
        ├── Core/
        │   ├── GameManager.cs       ← State machine, lives, scene loading
        │   ├── ScoreManager.cs      ← Score, streak multiplier, high score
        │   ├── AudioManager.cs      ← Procedural SFX + music
        │   └── ObjectPool.cs        ← Generic tag-based pool
        ├── Player/
        │   ├── PlayerController.cs  ← WASD/Arrow movement
        │   ├── PlayerHealth.cs      ← HP, shield, i-frames, death
        │   └── PlayerShooter.cs     ← Single/Double/Triple fire modes
        ├── Enemy/
        │   ├── EnemyBase.cs         ← HP, score, death, power-up drop
        │   ├── EnemyController.cs   ← 5 movement patterns
        │   ├── EnemyShooter.cs      ← 6 bullet patterns
        │   ├── EnemySpawner.cs      ← Formation helper
        │   └── WaveManager.cs       ← Wave data + progression + boss logic
        ├── Projectiles/
        │   └── Bullet.cs            ← Shared player/enemy bullet
        ├── PowerUps/
        │   ├── PowerUp.cs           ← 5 types + effect application
        │   └── PowerUpSpawner.cs    ← Drop-at-death logic
        ├── UI/
        │   ├── UIManager.cs         ← HUD, pause panel, game-over panel
        │   └── MainMenuController.cs← Main menu scene
        ├── Background/
        │   ├── ParallaxLayer.cs     ← Infinite vertical scroll
        │   └── StarField.cs         ← Procedural particle star field
        └── Utility/
            ├── SpriteFactory.cs     ← All sprites generated at runtime
            ├── SpriteApplier.cs     ← Choose sprite type in Inspector
            ├── CameraShake.cs       ← Coroutine screen shake
            ├── Explosion.cs         ← Scale/fade explosion VFX
            └── BoundaryDestroyer.cs ← Auto-despawn off-screen objects
```

---

## 🏗️ Step-by-Step Setup

### 1. Create the Unity Project

1. Open **Unity Hub** → **New Project**
2. Choose **2D (Built-in Render Pipeline)**
3. Name it `SpaceShooter`
4. Copy all `.cs` files from this repository into `Assets/Scripts/` (preserving subfolders)

### 2. Import TextMeshPro

`Window → TextMeshPro → Import TMP Essential Resources`

---

### 3. Build the Scenes

You need **two scenes**: `MainMenu` and `Game`.

Add both to **File → Build Settings → Scenes In Build** in order:
- Index 0: `MainMenu`
- Index 1: `Game`

---

### 4. MainMenu Scene

Create a new scene called `MainMenu` and build this hierarchy:

```
Main Camera
EventSystem (UI → Event System)
GameManagerHolder         ← Empty GO with GameManager.cs + ScoreManager.cs + AudioManager.cs
Canvas (Screen Space Overlay)
└─ MainMenuPanel
    ├─ TitleText           TMP_Text   "SPACE SHOOTER"   → assign to MainMenuController.titleText
    ├─ HighScoreText       TMP_Text   "BEST  ------"    → assign to MainMenuController.highScoreText
    ├─ PlayButton          Button     OnClick → MainMenuController.OnPlayClicked()
    └─ QuitButton          Button     OnClick → MainMenuController.OnQuitClicked()
```

Attach `MainMenuController.cs` to `Canvas` (or any GO).

---

### 5. Game Scene

Create a new scene called `Game` with this hierarchy:

```
Main Camera                      ← attach CameraShake.cs
  Tag: MainCamera, Size: 5

──── MANAGERS ────────────────────────────────────────────────
GameManagers (Empty GO)
  ├─ GameManager.cs   (playerPrefab=Player, startLives=3)
  ├─ ScoreManager.cs
  ├─ AudioManager.cs
  └─ WaveManager.cs   (basicEnemyPrefab, fastEnemyPrefab, heavyEnemyPrefab, bossEnemyPrefab)

PowerUpManager (Empty GO)
  └─ PowerUpSpawner.cs  (assign all 5 power-up prefabs)

EnemySpawner (Empty GO)
  └─ EnemySpawner.cs

──── BACKGROUND ──────────────────────────────────────────────
StarField (Empty GO)
  └─ StarField.cs (+ auto-created ParticleSystem)

BgLayer1 (Sprite: 10×12 solid dark-blue)   speed=0.6    sortingOrder=-5
  └─ ParallaxLayer.cs

BgLayer2 (Sprite: 10×12 solid dark-purple) speed=1.2    sortingOrder=-4
  └─ ParallaxLayer.cs

──── PLAYER (Prefab) ─────────────────────────────────────────
Player
  Tag: Player
  Components:
    SpriteApplier.cs     (type = PlayerShip)
    SpriteRenderer
    Rigidbody2D          (Gravity=0, Freeze Rotation=true)
    PolygonCollider2D    (Is Trigger = true)
    PlayerController.cs
    PlayerHealth.cs      (shieldVisual = ShieldChild)
    PlayerShooter.cs     (bulletPrefab = PlayerBullet prefab)
  └─ ShieldChild (Empty)
       SpriteApplier (type = Shield)
       SpriteRenderer
       CircleCollider2D

──── PREFABS ─────────────────────────────────────────────────

PlayerBullet
  Tag: PlayerBullet, Layer: Default
  SpriteApplier (PlayerBullet)
  Rigidbody2D (Gravity=0)
  CapsuleCollider2D (IsTrigger=true)
  Bullet.cs

EnemyBullet
  Tag: EnemyBullet
  SpriteApplier (EnemyBullet)
  Rigidbody2D (Gravity=0)
  CapsuleCollider2D (IsTrigger=true)
  Bullet.cs

BasicEnemy
  Tag: Enemy
  SpriteApplier (BasicEnemy)
  Rigidbody2D (Gravity=0, Kinematic)
  PolygonCollider2D (IsTrigger=true)
  EnemyBase.cs      (maxHP=2, scoreValue=100, dropChance=0.15)
  EnemyController.cs
  EnemyShooter.cs   (bulletPrefab = EnemyBullet)
  BoundaryDestroyer.cs

FastEnemy
  Tag: Enemy
  SpriteApplier (FastEnemy)
  EnemyBase.cs      (maxHP=1, scoreValue=150, dropChance=0.10)
  EnemyController.cs (speed=4.5, pattern=StraightDown)
  EnemyShooter.cs   (fireRate=2.5)
  BoundaryDestroyer.cs

HeavyEnemy
  Tag: Enemy
  SpriteApplier (HeavyEnemy)
  EnemyBase.cs      (maxHP=6, scoreValue=300, dropChance=0.30)
  EnemyController.cs (speed=1.5, pattern=SineWave)
  EnemyShooter.cs   (pattern=Spread3, fireRate=2.0)
  BoundaryDestroyer.cs

Boss
  Tag: Enemy
  SpriteApplier (Boss)
  EnemyBase.cs      (maxHP=40, scoreValue=2000, dropChance=1.0)
  EnemyController.cs (pattern=BossPatrol)
  EnemyShooter.cs   (pattern=Circle8, fireRate=1.2)
  BoundaryDestroyer.cs

PowerUpRapidFire / TripleShot / Shield / Heal / SpeedBoost
  SpriteApplier (PowerUp) – colour set at runtime
  CircleCollider2D (IsTrigger=true)
  PowerUp.cs (set powerUpType for each)

──── UI ──────────────────────────────────────────────────────
Canvas (Screen Space – Overlay)
├─ HUD
│   ├─ ScoreText         TMP_Text  top-left   → UIManager.scoreText
│   ├─ MultiplierText    TMP_Text  next to it → UIManager.multiplierText
│   ├─ WaveText          TMP_Text  top-right  → UIManager.waveText
│   ├─ LivesPanel        Horizontal Layout   → UIManager.livesPanel
│   └─ HealthBar
│       ├─ Background    Image (dark grey)
│       └─ Fill          Image, ImageType=Filled, FillMethod=Horizontal
│                                             → UIManager.healthBarFill
├─ WaveBanner            TMP_Text  centred, size 48 → UIManager.waveBannerText
├─ PowerUpText           TMP_Text  centred, size 32 → UIManager.powerUpText
├─ BossHPRoot            (Panel at top)
│   └─ BossFill          Image Filled        → UIManager.bossHPFill
├─ PausePanel            (full-screen dark overlay)
│   ├─ PausedText        TMP_Text
│   ├─ ResumeButton      → UIManager.OnResumeClicked()
│   └─ MenuButton        → UIManager.OnMenuClicked()
└─ GameOverPanel         (full-screen overlay)
    ├─ GameOverText      TMP_Text "GAME OVER"
    ├─ FinalScoreText    TMP_Text → UIManager.finalScoreText
    ├─ HighScoreText     TMP_Text → UIManager.highScoreText
    ├─ RestartButton     → UIManager.OnRestartClicked()
    └─ MenuButton        → UIManager.OnMenuClicked()
```

Assign `UIManager.cs` to the Canvas GO and wire every field in the Inspector.

---

### 6. Physics Layers & Tags

In **Edit → Tags and Layers** add:

**Tags:** `Player`, `Enemy`, `PlayerBullet`, `EnemyBullet`

**Layer Collision Matrix** (Edit → Project Settings → Physics 2D):
- Player collides with: EnemyBullet, Enemy, PowerUp
- PlayerBullet collides with: Enemy
- EnemyBullet collides with: Player
- Enemy does NOT collide with Enemy (avoids stacking)

---

### 7. Controls

| Key | Action |
|-----|--------|
| WASD / Arrow Keys | Move |
| Space / Z | Shoot (hold to auto-fire) |
| Escape | Pause / Resume |

---

## 🖥️ Building to Windows .EXE

1. **File → Build Settings**
2. Platform: **PC, Mac & Linux Standalone**
3. Target Platform: **Windows**
4. Architecture: **x86_64**
5. Click **Build** → choose output folder
6. Unity generates `SpaceShooter.exe` + `SpaceShooter_Data/` folder

> ⚠️ Distribute the `.exe` **and** `_Data` folder together.
> The game runs standalone — no Unity installation needed on the target PC.

---

## 🎮 Gameplay Loop

```
MainMenu → Play → Game Scene
  Wave 1 spawns → kill all enemies → Wave 2
  Every 5th wave = BOSS (bigger, more HP, circle bullet pattern)
  Player dies → lose a life → respawn (if lives > 0)
  All lives lost → Game Over → score saved → Return to Menu
```

---

## ✨ Feature Summary

| Feature | Implementation |
|---------|---------------|
| Movement | WASD / Arrow keys, screen-clamped |
| Shooting | Hold Space – single/double/triple modes |
| Fire modes | Upgraded via TripleShot power-up |
| Enemy AI | 5 patterns: straight, sine, zigzag, dive, boss-patrol |
| Bullet patterns | 6: single, spread-3, spread-5, aimed, circle-8, burst |
| Wave system | Infinite, difficulty scales every wave |
| Boss waves | Wave 5, 10, 15 … – HP bar shown, large explosion |
| Power-ups | 5 types dropped on enemy death (15% chance) |
| Score multiplier | x1→x4 based on kill streak |
| High score | Saved via PlayerPrefs |
| Audio | 100% procedural – square wave, sawtooth, noise |
| Sprites | 100% procedural – Texture2D drawn in code |
| Parallax | 2-layer infinite scroll background |
| Stars | ParticleSystem configured at runtime |
| Camera shake | Coroutine-based, magnitude + duration |
| Explosion VFX | Scale-up + fade-out, colour-shifts |
