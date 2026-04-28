# Space Shooter (Unity, Windows Desktop)

A complete 2D Unity space-shooter project targeting **Windows Standalone**.

## Unity Compatibility
- **Unity 2020.3 LTS or newer** (tested design for 2020.3+ API compatibility).
- Uses **legacy Input Manager** (`Input.GetAxisRaw`, `Input.GetKey`).
- Render mode: 2D, 1920x1080, windowed mode.

## Project Structure

```text
Assets/
  Audio/
  Prefabs/
  Scenes/
  Scripts/
    AudioManager.cs
    BulletSystem.cs
    EnemyManager.cs
    GameManager.cs
    MenuManager.cs
    ParallaxBackground.cs
    PlayerController.cs
    PowerUpSystem.cs
    UIManager.cs
  Sprites/
  UI/
ProjectSettings/
Assembly-CSharp.csproj
Space-Shooter.sln
README.md
```

## Core Systems Included
- Player movement (arrow keys/WASD), shooting (space), health, death.
- Enemy wave manager with **5 escalating waves** and movement patterns.
- Bullet system for player/enemy bullets, damage, lifetime.
- Game state handling: start, pause, game over, victory.
- HUD + menu screens integration.
- Power-ups: shield, rapid fire, health restore.
- Audio manager for SFX and background music.
- Main menu, pause menu, game over / victory controls.
- Scrolling parallax background system.

---

## Sprite Placeholder Art Direction (Pixel Art)
Create these as 32x32 / 64x64 pixel-art sprites with hard edges and limited palette:

1. **Player Ship** (`player_ship.png`)
   - Triangular blue/cyan ship, brighter cockpit pixel highlight, rear engine glow.
2. **Enemy Basic** (`enemy_basic.png`)
   - Red diamond-shaped fighter with small wing tips.
3. **Enemy Fast/ZigZag** (`enemy_zigzag.png`)
   - Orange slim craft with angled wings suggesting agility.
4. **Enemy Tank** (`enemy_tank.png`)
   - Purple/gray heavy blocky ship, thicker armor outline.
5. **Player Bullet** (`player_bullet.png`)
   - Thin cyan vertical bolt, bright center pixel line.
6. **Enemy Bullet** (`enemy_bullet.png`)
   - Red/orange plasma orb with 1px glow border.
7. **Power-up Shield** (`powerup_shield.png`)
   - Cyan hexagonal icon with shield emblem.
8. **Power-up Rapid Fire** (`powerup_rapidfire.png`)
   - Yellow ammo/lightning symbol.
9. **Power-up Health** (`powerup_health.png`)
   - Green cross in a rounded capsule.
10. **Background Layer 1** (`bg_layer1.png`)
    - Sparse stars, dark navy base.
11. **Background Layer 2** (`bg_layer2.png`)
    - Nebula patches + denser tiny stars for depth.

---

## Prefab Setup Instructions

### 1) Player Prefab (`Assets/Prefabs/Player.prefab`)
- Components:
  - `SpriteRenderer` (player sprite)
  - `Collider2D` (set `Is Trigger` true)
  - `PlayerController`
- Child object `FirePoint` at top-center of ship and assign to `PlayerController.firePoint`.
- Tag: `Player`
- Layer: `Player`

### 2) Enemy Prefab (`Assets/Prefabs/Enemy.prefab`)
- Components:
  - `SpriteRenderer`
  - `Collider2D` (`Is Trigger` true)
- `EnemyManager` adds/configures runtime enemy behavior at spawn.
- Tag: `Enemy`
- Layer: `Enemy`

### 3) Player Bullet Prefab (`Assets/Prefabs/PlayerBullet.prefab`)
- Components:
  - `SpriteRenderer`
  - `Collider2D` (`Is Trigger` true)
- Tag: `PlayerBullet`
- Layer: `PlayerProjectile`

### 4) Enemy Bullet Prefab (`Assets/Prefabs/EnemyBullet.prefab`)
- Components:
  - `SpriteRenderer`
  - `Collider2D` (`Is Trigger` true)
- Tag: `EnemyBullet`
- Layer: `EnemyProjectile`

### 5) Power-up Prefabs
- `PowerUp_Shield.prefab`, `PowerUp_RapidFire.prefab`, `PowerUp_Health.prefab`
- Components:
  - `SpriteRenderer`
  - `Collider2D` (`Is Trigger` true)
- Tag: `PowerUp`
- Layer: `PowerUp`

### 6) Manager Object Prefab (optional)
Create one scene object `Systems` with these scripts:
- `GameManager`
- `EnemyManager`
- `BulletSystem`
- `PowerUpSystem`
- `AudioManager`
- `UIManager`
- `MenuManager`

Wire all references in Inspector (prefabs, UI texts, audio clips).

---

## Scene Setup

## 1) `MainMenu` Scene (`Assets/Scenes/MainMenu.unity`)
- Canvas with:
  - Title text: "SPACE SHOOTER"
  - Start button -> `MenuManager.StartGameFromMainMenu()`
  - Quit button -> `MenuManager.QuitGame()`
- Add `MenuManager` and optional looping background.

## 2) `GamePlay` Scene (`Assets/Scenes/GamePlay.unity`)
- Camera: Orthographic, sized for 16:9 gameplay area.
- Add background layers and `ParallaxBackground` script.
- Add Player prefab near bottom center.
- Add manager object with all required systems.
- Add HUD canvas:
  - Score, Health, Wave, Shield status, Rapid Fire status
  - Pause panel (Resume, Main Menu buttons)
  - End panel (Win/Lose title, score, Restart/Main Menu/Quit buttons)
- In GameManager Start state, main gameplay is hidden until start action if desired.

---

## Input System Configuration (Legacy)
In `Edit > Project Settings > Input Manager` ensure these axes exist:
- `Horizontal` (Left/Right arrows + A/D)
- `Vertical` (Up/Down arrows + W/S)
- Shooting uses `Space` via `Input.GetKey(KeyCode.Space)`.
- Pause uses `Escape`.

---

## Tags and Layers Setup
In `Edit > Project Settings > Tags and Layers`:

### Tags
- `Player`
- `Enemy`
- `PlayerBullet`
- `EnemyBullet`
- `PowerUp`

### Layers
- `Player`
- `Enemy`
- `PlayerProjectile`
- `EnemyProjectile`
- `PowerUp`

### Physics2D Collision Matrix
Enable collisions:
- Player <-> Enemy
- Player <-> EnemyProjectile
- Player <-> PowerUp
- Enemy <-> PlayerProjectile
Disable unnecessary combinations (projectile-projectile, enemy-enemy, etc.) to reduce checks.

---

## Windows Build Instructions (Standalone EXE)
1. Open project in Unity Hub.
2. Open `File > Build Settings`.
3. Add scenes in order:
   1. `Assets/Scenes/MainMenu.unity`
   2. `Assets/Scenes/GamePlay.unity`
4. Platform: select **PC, Mac & Linux Standalone**.
5. Target Platform: **Windows**.
6. Architecture: **x86_64**.
7. Player Settings:
   - Resolution: `1920 x 1080`
   - Fullscreen Mode: `Windowed`
   - Default is Native Resolution: Off
8. Click **Build**, choose output folder (for example `Builds/Windows`).
9. Run generated `Space-Shooter.exe`.

---

## Step-by-Step Compilation / Run Guide
1. Clone repo.
2. Open folder with Unity 2020.3+.
3. Let Unity import assets and compile scripts.
4. Create the two scenes and prefabs as described above.
5. Assign prefabs to `BulletSystem`, `EnemyManager`, and `PowerUpSystem` in inspector.
6. Assign UI text references in `UIManager`.
7. Assign clips and sources in `AudioManager`.
8. Enter Play mode and test all 5 waves.
9. Build for Windows using steps above.

---

## `.csproj` / `.sln` Notes
- Unity auto-generates and refreshes solution/project files.
- This repo includes baseline references (`Assembly-CSharp.csproj`, `Space-Shooter.sln`) for IDE indexing.
- If Unity regenerates them, that is expected.
