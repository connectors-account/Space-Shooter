# 🚀 Space Shooter — Unity Desktop Game

A classic top-down space shooter game built with Unity for Windows desktop. Fight through 5 waves of increasingly difficult enemies, collect power-ups, and rack up the highest score!

---

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/           # All C# game scripts
│   │   ├── GameManager.cs         # Game state, scoring, wave & life management
│   │   ├── PlayerController.cs    # Player movement, shooting, health, power-ups
│   │   ├── EnemyController.cs     # Enemy AI, 3 types, shooting, drops
│   │   ├── BulletController.cs    # Bullet movement and collision
│   │   ├── SpawnManager.cs        # Wave-based enemy spawning
│   │   ├── PowerUpController.cs   # Power-up drift and effect application
│   │   ├── ParallaxBackground.cs  # Infinite scrolling background
│   │   ├── MenuManager.cs         # Main menu & game over UI
│   │   ├── HealthUI.cs            # Lives display (text + icons)
│   │   ├── ScoreUI.cs             # Score & wave display
│   │   └── SceneSetup.cs          # Runtime scene bootstrapper
│   ├── Sprites/            # PNG sprite assets
│   ├── Resources/Sprites/  # Runtime-loadable sprite copies
│   ├── Scenes/             # Unity scene files
│   │   ├── MainMenu.unity
│   │   ├── GamePlay.unity
│   │   └── GameOver.unity
│   ├── Prefabs/            # (Created at runtime by SceneSetup)
│   ├── Audio/              # Drop .wav/.ogg files here for SFX
│   ├── Materials/
│   └── Animations/
├── ProjectSettings/        # Unity project configuration
│   ├── ProjectSettings.asset
│   ├── TagManager.asset
│   ├── InputManager.asset
│   ├── EditorBuildSettings.asset
│   ├── QualitySettings.asset
│   ├── Physics2DSettings.asset
│   ├── AudioManager.asset
│   └── TimeManager.asset
├── Packages/
│   └── manifest.json
└── README.md
```

---

## 🎮 Game Features

### Gameplay
- **5 progressive waves** of enemies with increasing difficulty
- **3 enemy types:**
  - 🔴 **Straight** — Moves directly downward (1 HP, 100 pts)
  - 🟢 **Zigzag** — Weaves side-to-side while descending (2 HP, 200 pts)
  - 🟣 **Chaser** — Pursues the player, aims bullets (3 HP, 350 pts)
- **2 power-ups:**
  - 💚 **Health Restore** — Restores 1 life
  - ⚡ **Rapid Fire** — Doubles fire rate for 5 seconds
- **Collision system** using Unity 2D trigger colliders
- **Parallax scrolling** starfield background

### UI / Screens
- **Main Menu** — Play and Quit buttons with controls info
- **HUD** — Score, wave number, and lives display
- **Game Over / Victory** — Final score, wave reached, restart or return to menu

---

## 🕹️ Controls

| Action | Key(s) |
|--------|--------|
| Move Up | `W` or `↑` |
| Move Down | `S` or `↓` |
| Move Left | `A` or `←` |
| Move Right | `D` or `→` |
| Shoot | `Space` (hold for continuous fire) |

---

## 🛠️ How to Open in Unity

### Prerequisites
- **Unity Hub** installed ([Download](https://unity.com/download))
- **Unity Editor 2022.3 LTS** or newer (any 2022.3.x+ or 2023.x will work)

### Steps

1. **Open Unity Hub**

2. **Add Existing Project**
   - Click `Open` → `Add project from disk`
   - Navigate to the `space_shooter_game` folder
   - Select it and click `Open`

3. **Let Unity import** — Unity will generate the Library folder and import all assets. This may take a few minutes on first open.

4. **Configure Scenes in Build Settings** (if not already set):
   - Go to `File` → `Build Settings`
   - Click `Add Open Scenes` for each scene, or drag scenes from `Assets/Scenes/`:
     - `MainMenu` (index 0)
     - `GamePlay` (index 1)
     - `GameOver` (index 2)

5. **Configure Tags** (if not auto-imported):
   - Go to `Edit` → `Project Settings` → `Tags and Layers`
   - Ensure these tags exist: `Player`, `PlayerBullet`, `EnemyBullet`, `Enemy`, `PowerUp`

6. **Add SceneSetup to each scene**:
   - Open each scene (`MainMenu`, `GamePlay`, `GameOver`)
   - Create an empty GameObject named `SceneBootstrap`
   - Add the `SceneSetup` script component to it
   - Save the scene

7. **Play** — Press the ▶️ Play button to test!

---

## 🏗️ How to Build for Windows

1. Open the project in Unity Editor

2. Go to `File` → `Build Settings`

3. Select **Windows, Mac, Linux** as the platform
   - If not installed, click `Install with Unity Hub` and add the Windows Build Support module

4. Ensure all 3 scenes are listed and checked:
   ```
   0: Scenes/MainMenu
   1: Scenes/GamePlay
   2: Scenes/GameOver
   ```

5. Set **Target Platform** to `Windows`

6. Set **Architecture** to `x86_64`

7. Click **Build** or **Build and Run**

8. Choose an output folder (e.g., `Builds/Windows`)

9. Unity will generate:
   ```
   SpaceShooter.exe          ← Run this!
   SpaceShooter_Data/        ← Game data (must be in same folder)
   UnityPlayer.dll           ← Unity runtime
   ```

10. **Distribute** — Zip the entire output folder to share the game!

---

## 🔊 Adding Sound Effects

The game has AudioSource components pre-configured on all objects. To add sounds:

1. Drop `.wav` or `.ogg` files into `Assets/Audio/`
2. In the Unity Inspector, assign audio clips to:
   - **Player**: `Shoot SFX`, `Hit SFX`, `PowerUp SFX`
   - **Enemies**: `Hit SFX`
   - **Menus**: `Button Click Audio`

Recommended sounds to add:
- `shoot.wav` — laser/blaster sound
- `explosion.wav` — enemy/player death
- `powerup.wav` — power-up pickup
- `hit.wav` — damage taken
- `button_click.wav` — UI button press
- `background_music.ogg` — looping background track

---

## 🎨 Sprite Assets Included

| Sprite | Size | Description |
|--------|------|-------------|
| `player_ship.png` | 64×64 | Blue triangular player ship |
| `enemy_straight.png` | 48×48 | Red diamond enemy |
| `enemy_zigzag.png` | 48×48 | Green inverted triangle |
| `enemy_chaser.png` | 48×48 | Purple spiked circle |
| `bullet_player.png` | 16×16 | Cyan energy bolt |
| `bullet_enemy.png` | 16×16 | Orange energy bolt |
| `powerup_health.png` | 32×32 | Green circle with cross |
| `powerup_rapidfire.png` | 32×32 | Yellow circle with lightning |
| `bg_layer1_stars.png` | 512×1024 | Deep space star field |
| `bg_layer2_nebula.png` | 512×1024 | Nebula overlay |
| `heart.png` | 32×32 | Red heart for lives UI |

---

## 📝 Architecture Notes

### SceneSetup.cs (Runtime Bootstrapper)
Since Unity scene files require the Editor to fully configure GameObjects and component references, the `SceneSetup.cs` script programmatically creates all GameObjects, components, and wiring at runtime. This means:
- The game works even if scene files only contain a Camera and the SceneSetup script
- All prefabs are created as inactive templates and instantiated by the SpawnManager
- UI is built programmatically with proper Canvas, Text, and Button setup

### Key Design Patterns
- **Singleton** — `GameManager` uses a singleton for global state access
- **Event-driven UI** — Score/Lives/Wave changes fire C# events that UI scripts subscribe to
- **Component-based** — Each behavior is a separate MonoBehaviour for modularity
- **Template pattern** — Enemy/bullet/power-up "prefabs" created as inactive GameObjects

---

## ⚙️ Configuration

Game balance can be tuned in the Unity Inspector or by modifying these values:

| Parameter | Location | Default |
|-----------|----------|---------|
| Player speed | PlayerController | 8 |
| Fire rate | PlayerController | 0.25s |
| Rapid fire rate | PlayerController | 0.1s |
| Starting lives | GameManager | 3 |
| Total waves | GameManager | 5 |
| Power-up drop chance | EnemyController | 20% |
| Background scroll speed | ParallaxBackground | 0.5 |

---

## 📋 Requirements

- **Unity**: 2022.3 LTS or newer
- **Platform**: Windows 10/11 (x86_64)
- **.NET**: Included with Unity (no separate install needed)
- **Minimum specs**: Any PC that can run Unity games

---

## 📄 License

This project is provided as-is for educational and personal use.
