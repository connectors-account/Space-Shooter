# Space Shooter - Complete Unity Setup Guide

This guide walks you through setting up the Space Shooter game in Unity Editor step-by-step.

---

## Prerequisites

- **Unity Hub** installed ([Download](https://unity.com/download))
- **Unity Editor 2021.3 LTS or newer** (2022.3 LTS recommended)
- **Windows 10/11** for building Windows executables

---

## Step 1: Open the Project in Unity

1. Open **Unity Hub**
2. Click **"Open"** (or "Add project from disk")
3. Navigate to and select the `space_shooter_game` folder
4. Unity will detect it as a project and import all assets
5. Wait for the initial import to complete (may take 1-2 minutes)

> **Note:** If Unity asks about the render pipeline, select **Built-in Render Pipeline** (default).

---

## Step 2: Configure Tags and Layers

Unity needs custom tags for collision detection. Go to **Edit → Project Settings → Tags and Layers**:

### Tags (add these if not already present):
| Tag Name | Purpose |
|---|---|
| `Player` | Player ship |
| `PlayerBullet` | Bullets fired by the player |
| `EnemyBullet` | Bullets fired by enemies |
| `Enemy` | All enemy ships |
| `PowerUp` | All power-up pickups |

### Sorting Layers (add in order):
| Layer Name | Order |
|---|---|
| `Background` | 0 (bottom) |
| `Default` | 1 |
| `Foreground` | 2 |
| `UI` | 3 (top) |

---

## Step 3: Configure Sprite Import Settings

For each sprite in `Assets/Sprites/`:

1. Select the sprite file in the Project window
2. In the Inspector, set:
   - **Texture Type:** Sprite (2D and UI)
   - **Pixels Per Unit:** 100
   - **Filter Mode:** Point (for crisp pixel look) or Bilinear
   - **Compression:** None
3. Click **Apply**

---

## Step 4: Create the Main Game Scene

### 4.1 Create a New Scene
1. Go to **File → New Scene** (select "Basic 2D")
2. Save it as `Assets/Scenes/GameScene.unity`

### 4.2 Set Up the Camera
1. Select **Main Camera** in the Hierarchy
2. Set **Background Color** to dark blue/black: `(8, 8, 20, 255)` or `#08081`
3. Set **Size** to `5.5` (for the orthographic camera)
4. Set **Position** to `(0, 0, -10)`
5. Add the `ScreenBounds` script to the camera

### 4.3 Create the GameManager Object
1. Create an **Empty GameObject**: `GameObject → Create Empty`
2. Name it `GameManager`
3. Add the script: **Add Component → SpaceShooter.Managers.GameManager**
4. This is a singleton that persists across scenes

### 4.4 Create the AudioManager Object
1. Create an **Empty GameObject** named `AudioManager`
2. Add the script: **Add Component → SpaceShooter.Managers.AudioManager**
3. Add **two AudioSource** components to it
4. Assign the first AudioSource to the `Sfx Source` field
5. Assign the second AudioSource to the `Music Source` field (check **Loop**)
6. Drag and drop audio clips from `Assets/Audio/` into the corresponding fields:
   - `Shoot Clip` ← `shoot.wav`
   - `Explosion Clip` ← `explosion.wav`
   - `Power Up Clip` ← `powerup.wav`
   - `Player Hit Clip` ← `player_hit.wav`
   - `Wave Start Clip` ← `wave_start.wav`
   - `Game Over Clip` ← `game_over.wav`
   - `Button Click Clip` ← `button_click.wav`

### 4.5 Create the SpawnManager Object
1. Create an **Empty GameObject** named `SpawnManager`
2. Add the script: **Add Component → SpaceShooter.Managers.SpawnManager**
3. We'll assign enemy prefabs after creating them (Step 5)

---

## Step 5: Create Prefabs

### 5.1 Player Ship Prefab
1. Create an **Empty GameObject** named `Player`
2. Set **Tag** to `Player`
3. Set **Position** to `(0, -3, 0)`
4. Add **SpriteRenderer**: Assign `player_ship.png` sprite; Sorting Layer: `Default`; Order: 5
5. Add **BoxCollider2D**: Check `Is Trigger`; adjust size to fit the ship sprite (~0.5 x 0.5)
6. Add **Rigidbody2D**: Set **Body Type** to `Kinematic` (prevents physics forces)
7. Add script: **SpaceShooter.Player.PlayerController**
8. Create a **child Empty GameObject** named `FirePoint`, position at `(0, 0.4, 0)` (front of ship)
9. Create a **child Empty GameObject** named `ShieldVisual`:
   - Add **SpriteRenderer** with `shield_bubble.png`
   - Disable the ShieldVisual object (uncheck the checkbox)
10. In PlayerController inspector:
    - Drag `FirePoint` to the `Fire Point` field
    - Drag `ShieldVisual` to the `Shield Visual` field
    - Drag the SpriteRenderer to the `Sprite Renderer` field
    - Set `Bullet Prefab` (after creating bullet prefab below)
11. **Drag the Player from Hierarchy into `Assets/Prefabs/`** to create a prefab
12. **Keep one instance in the scene**

### 5.2 Player Bullet Prefab
1. Create an **Empty GameObject** named `PlayerBullet`
2. Set **Tag** to `PlayerBullet`
3. Add **SpriteRenderer**: Assign `player_bullet.png`; Sorting Layer: `Default`; Order: 3
4. Add **BoxCollider2D**: Check `Is Trigger`; adjust size
5. Add **Rigidbody2D**: Body Type = `Kinematic`
6. Add script: **SpaceShooter.Weapons.BulletController**
   - Speed: `12`
   - Damage: `10`
   - Direction: `(0, 1)` (upward)
7. **Drag into `Assets/Prefabs/`** to create prefab
8. **Delete from scene** (it's spawned at runtime)

### 5.3 Enemy Bullet Prefab
1. Create an **Empty GameObject** named `EnemyBullet`
2. Set **Tag** to `EnemyBullet`
3. Add **SpriteRenderer**: Assign `enemy_bullet.png`; Sorting Layer: `Default`; Order: 3
4. Add **BoxCollider2D**: Check `Is Trigger`
5. Add **Rigidbody2D**: Body Type = `Kinematic`
6. Add script: **SpaceShooter.Weapons.BulletController**
   - Speed: `6`
   - Damage: `10`
   - Direction: `(0, -1)` (downward)
7. **Drag into `Assets/Prefabs/`**, then **delete from scene**

### 5.4 Enemy Prefabs (3 types)

For each enemy type, follow these steps:

#### Basic Enemy
1. Create Empty GameObject named `EnemyBasic`
2. Tag: `Enemy`
3. Add **SpriteRenderer**: `enemy_basic.png`; Sorting Layer: `Default`; Order: 4
4. Add **BoxCollider2D**: `Is Trigger` = true
5. Add **Rigidbody2D**: Body Type = `Kinematic`
6. Add script: **SpaceShooter.Enemy.EnemyController**
   - Enemy Type: `Basic`
   - Max Health: `30`
   - Move Speed: `3`
   - Score Value: `100`
   - Fire Rate: `1.5`
7. Create child `FirePoint` at `(0, -0.3, 0)` (front of enemy, facing down)
8. Assign `EnemyBullet` prefab to `Bullet Prefab` field
9. Assign `FirePoint` to the `Fire Point` field
10. Assign power-up prefabs to `Power Up Prefabs` array (after creating them)
11. **Drag into `Assets/Prefabs/`**, delete from scene

#### Zigzag Enemy
- Same as Basic but use `enemy_zigzag.png`
- Name: `EnemyZigzag`
- Enemy Type: `Zigzag`
- Max Health: `20`, Move Speed: `4`, Score Value: `150`, Fire Rate: `1.0`

#### Tank Enemy
- Same as Basic but use `enemy_tank.png`
- Name: `EnemyTank`
- Enemy Type: `Tank`
- Max Health: `80`, Move Speed: `1.5`, Score Value: `300`, Fire Rate: `2.5`

### 5.5 Power-Up Prefabs (3 types)

For each power-up:

#### Health Pack
1. Create Empty GameObject named `PowerUpHealth`
2. Tag: `PowerUp`
3. Add **SpriteRenderer**: `powerup_health.png`; Order: 6
4. Add **CircleCollider2D**: `Is Trigger` = true
5. Add **Rigidbody2D**: Body Type = `Kinematic`
6. Add script: **SpaceShooter.PowerUps.PowerUpController**
   - Power Up Type: `HealthPack`
   - Heal Amount: `30`
7. **Drag into `Assets/Prefabs/`**, delete from scene

#### Rapid Fire
- Same setup, use `powerup_rapidfire.png`
- Name: `PowerUpRapidFire`
- Power Up Type: `RapidFire`

#### Shield
- Same setup, use `powerup_shield.png`
- Name: `PowerUpShield`
- Power Up Type: `Shield`

### 5.6 Assign Cross-References
Now go back and connect the prefabs:

1. **Player prefab** → Set `Bullet Prefab` to `PlayerBullet` prefab
2. **All Enemy prefabs** → Set `Bullet Prefab` to `EnemyBullet` prefab
3. **All Enemy prefabs** → Set `Power Up Prefabs` array to include all 3 power-up prefabs
4. **SpawnManager** → Set:
   - `Basic Enemy Prefab` → `EnemyBasic`
   - `Zigzag Enemy Prefab` → `EnemyZigzag`
   - `Tank Enemy Prefab` → `EnemyTank`

---

## Step 6: Create the UI (Canvas)

### 6.1 Create Canvas
1. **GameObject → UI → Canvas**
2. Set Canvas Scaler:
   - **UI Scale Mode:** Scale With Screen Size
   - **Reference Resolution:** 1920 x 1080
   - **Match:** 0.5
3. Name it `UICanvas`

### 6.2 Create UIManager
1. Create an **Empty GameObject** named `UIManager` (can be child of Canvas or separate)
2. Add script: **SpaceShooter.UI.UIManager**

### 6.3 Main Menu Panel
1. Under Canvas, create **UI → Panel** named `MainMenuPanel`
2. Set background to semi-transparent dark: `(10, 10, 30, 230)`
3. Add children:
   - **UI → Text** named `TitleText`: "SPACE SHOOTER", font size 72, centered, white, bold
   - **UI → Text** named `HighScoreMenuText`: "High Score: 0", font size 24
   - **UI → Button** named `StartButton`: Text = "START GAME", font size 32
   - **UI → Button** named `QuitButton`: Text = "QUIT", font size 24

### 6.4 HUD Panel
1. Create **UI → Panel** named `HUDPanel` (set alpha to 0 for transparent background)
2. Add children:
   - **UI → Text** named `ScoreText`: "Score: 0", top-left anchor, font size 28, white
   - **UI → Text** named `WaveText`: "Wave 1", top-center, font size 24
   - **UI → Slider** named `HealthBar`: bottom-left, set colors (fill = green), width ~300
   - **UI → Text** named `HealthText`: "100 / 100", near health bar
   - **UI → Image** named `ShieldIcon`: small blue icon (use shield sprite), top-right area
   - **UI → Image** named `RapidFireIcon`: small orange icon, next to shield icon
3. Start with HUD Panel **disabled** (unchecked)

### 6.5 Game Over Panel
1. Create **UI → Panel** named `GameOverPanel` (semi-transparent dark background)
2. Add children:
   - **UI → Text** named `GameOverTitleText`: "GAME OVER", font size 64, red, centered
   - **UI → Text** named `FinalScoreText`: "Score: 0", font size 36
   - **UI → Text** named `FinalWaveText`: "Wave Reached: 1", font size 28
   - **UI → Text** named `HighScoreText`: "High Score: 0", font size 24
   - **UI → Text** named `NewHighScoreText`: "NEW HIGH SCORE!", font size 32, yellow (start disabled)
   - **UI → Button** named `RestartButton`: Text = "PLAY AGAIN", font size 28
   - **UI → Button** named `MenuButton`: Text = "MAIN MENU", font size 24
3. Start with GameOver Panel **disabled**

### 6.6 Pause Panel
1. Create **UI → Panel** named `PausePanel` (semi-transparent)
2. Add children:
   - **UI → Text**: "PAUSED", font size 48, centered
   - **UI → Button** named `ResumeButton`: Text = "RESUME"
   - **UI → Button** named `PauseMenuButton`: Text = "MAIN MENU"
3. Start **disabled**

### 6.7 Wave Announcement Panel
1. Create **UI → Panel** named `WaveAnnouncementPanel` (transparent bg)
2. Add **UI → Text** named `WaveAnnouncementText`: "WAVE 1", font size 56, centered, white with outline
3. Start **disabled**

### 6.8 Wire Up UIManager
Select the `UIManager` object and drag all the panels and UI elements into the corresponding fields in the Inspector:
- Main Menu Panel, HUD Panel, Game Over Panel, Pause Panel, Wave Announcement Panel
- All Text fields, Buttons, Slider, Images as labeled

---

## Step 7: Set Up Background

### 7.1 Star Field (Procedural)
1. Create an **Empty GameObject** named `StarField`
2. Add script: **SpaceShooter.Environment.StarField**
3. Set Position to `(0, 0, 5)` (behind everything)
4. Configure: Star Count = 200, Field Width = 20, Field Height = 15

### 7.2 Parallax Background (Optional - for richer visuals)
1. Create an **Empty GameObject** named `ParallaxBackground`
2. Add script: **SpaceShooter.Environment.ParallaxBackground**
3. Create two child GameObjects:
   - `BG_Layer1`: Add SpriteRenderer with `bg_layer1.png`, Sorting Layer: `Background`, Order: 0, Scale to fill screen
   - `BG_Layer2`: Add SpriteRenderer with `bg_layer2.png`, Sorting Layer: `Background`, Order: 1, Scale to fill screen
4. Assign the SpriteRenderers to the script's Layer1/Layer2 fields

---

## Step 8: Final Scene Hierarchy Check

Your Hierarchy should look like:

```
GameScene
├── Main Camera (+ ScreenBounds script)
├── GameManager (+ GameManager script)
├── AudioManager (+ AudioManager script + 2x AudioSource)
├── SpawnManager (+ SpawnManager script)
├── Player (+ PlayerController, SpriteRenderer, BoxCollider2D, Rigidbody2D)
│   ├── FirePoint
│   └── ShieldVisual (disabled)
├── StarField (+ StarField script)
├── ParallaxBackground (+ ParallaxBackground script)
│   ├── BG_Layer1
│   └── BG_Layer2
├── UICanvas (Canvas + Canvas Scaler + Graphic Raycaster)
│   ├── UIManager (+ UIManager script)
│   ├── MainMenuPanel
│   │   ├── TitleText
│   │   ├── HighScoreMenuText
│   │   ├── StartButton
│   │   └── QuitButton
│   ├── HUDPanel (disabled)
│   │   ├── ScoreText
│   │   ├── WaveText
│   │   ├── HealthBar (Slider)
│   │   ├── HealthText
│   │   ├── ShieldIcon
│   │   └── RapidFireIcon
│   ├── GameOverPanel (disabled)
│   │   ├── GameOverTitleText
│   │   ├── FinalScoreText
│   │   ├── FinalWaveText
│   │   ├── HighScoreText
│   │   ├── NewHighScoreText
│   │   ├── RestartButton
│   │   └── MenuButton
│   ├── PausePanel (disabled)
│   │   ├── PausedText
│   │   ├── ResumeButton
│   │   └── PauseMenuButton
│   └── WaveAnnouncementPanel (disabled)
│       └── WaveAnnouncementText
└── EventSystem (auto-created with Canvas)
```

---

## Step 9: Test in Editor

1. Press **Play** in the Unity Editor
2. You should see:
   - Main menu with "START GAME" button
   - Click Start → HUD appears, player ship at bottom
   - Arrow keys / WASD to move, Space to shoot
   - Enemies spawn from top
   - Killing enemies gives score
   - Power-ups drop occasionally
   - Taking damage reduces health bar
   - Health = 0 → Game Over screen
   - ESC key pauses/unpauses

---

## Step 10: Build for Windows (.exe)

1. Go to **File → Build Settings**
2. Ensure your `GameScene` is in the **Scenes In Build** list
   - If not, click **Add Open Scenes**
3. Select **Platform: PC, Mac & Linux Standalone**
4. Set **Target Platform:** Windows
5. Set **Architecture:** x86_64
6. Click **Player Settings** and configure:
   - **Product Name:** Space Shooter
   - **Company Name:** (your name)
   - **Default Screen Width:** 1024
   - **Default Screen Height:** 768
   - **Fullscreen Mode:** Windowed (or Fullscreen Window)
7. Click **Build** (or **Build And Run**)
8. Choose an output folder (e.g., `Builds/Windows`)
9. Wait for the build to complete
10. Your `.exe` will be in the output folder along with a `_Data` folder

### Distributing the Build
To share your game, zip the entire output folder containing:
- `Space Shooter.exe`
- `Space Shooter_Data/` folder
- `UnityPlayer.dll`
- `MonoBleedingEdge/` folder

All files must stay together for the game to work.

---

## Troubleshooting

| Issue | Solution |
|---|---|
| Scripts won't compile | Ensure all script files are in `Assets/Scripts/` subfolders |
| Tags not found error | Manually add tags in Edit → Project Settings → Tags and Layers |
| Collisions not working | Ensure all colliders have `Is Trigger` checked and Rigidbody2D exists |
| Bullets not spawning | Check that bullet prefab is assigned in PlayerController and EnemyController |
| UI not showing | Ensure Canvas has EventSystem child, and UIManager fields are all assigned |
| No sound | Ensure AudioManager has AudioSource components and clips are assigned |
| Enemies not spawning | Check SpawnManager has all 3 enemy prefabs assigned |
| Power-ups not appearing | Check enemy prefabs have power-up prefabs in their array |

---

## Controls Summary

| Key | Action |
|---|---|
| Arrow Keys / WASD | Move ship |
| Spacebar | Fire weapon |
| ESC | Pause / Unpause |
