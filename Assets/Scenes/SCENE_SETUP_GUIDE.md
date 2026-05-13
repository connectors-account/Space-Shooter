# Scene Setup Guide

This document explains how to configure the three Unity scenes after opening
the project in the Unity Editor. The game is designed to self-bootstrap at
runtime via Setup scripts, but the scenes themselves need a minimal skeleton.

---

## Scene 1: MainMenu (Assets/Scenes/MainMenu.unity)

1. **Create the scene:** `File > New Scene > Basic (Built-in)`.
2. **Save as:** `Assets/Scenes/MainMenu.unity`.
3. **Create an empty GameObject** named `MainMenuBootstrap`.
4. **Attach** the `MainMenuSetup` script to it.
5. The `MainMenuSetup.Awake()` method creates the camera config, background,
   canvas, title, buttons, and wires the `MainMenuUI` component automatically.
6. **Delete** the default Directional Light (2D game — not needed).

### Objects in scene:
| GameObject         | Components          |
|--------------------|---------------------|
| Main Camera        | Camera (auto-configured) |
| MainMenuBootstrap  | MainMenuSetup       |

---

## Scene 2: Game (Assets/Scenes/Game.unity)

1. **Create the scene:** `File > New Scene > Basic (Built-in)`.
2. **Save as:** `Assets/Scenes/Game.unity`.
3. **Create an empty GameObject** named `GameBootstrap`.
4. **Attach** the `GameSetup` script to it.
5. `GameSetup.Awake()` creates:
   - Player (with sprites, physics, health, shooting, shield visual)
   - Enemy prefab templates (4 types)
   - Bullet prefab templates (player + enemy)
   - Power-up prefab templates (3 types)
   - EnemySpawner
   - 3-layer procedural parallax background
   - Full HUD canvas (health bar, shield bar, score, combo, wave, weapon level)
   - Pause menu overlay
6. **Delete** the default Directional Light.

### Objects in scene:
| GameObject       | Components    |
|------------------|---------------|
| Main Camera      | Camera (auto-configured) |
| GameBootstrap    | GameSetup     |

---

## Scene 3: GameOver (Assets/Scenes/GameOver.unity)

1. **Create the scene:** `File > New Scene > Basic (Built-in)`.
2. **Save as:** `Assets/Scenes/GameOver.unity`.
3. **Create an empty GameObject** named `GameOverBootstrap`.
4. **Attach** the `GameOverSetup` script to it.
5. `GameOverSetup.Awake()` creates the game-over canvas, score displays,
   new-high-score indicator, and Retry / Main Menu buttons.
6. **Delete** the default Directional Light.

### Objects in scene:
| GameObject          | Components      |
|---------------------|-----------------|
| Main Camera         | Camera (auto-configured) |
| GameOverBootstrap   | GameOverSetup   |

---

## Build Settings

After creating all three scenes, go to `File > Build Settings` and add them
in this order:

| Index | Scene                          |
|-------|--------------------------------|
| 0     | Assets/Scenes/MainMenu.unity   |
| 1     | Assets/Scenes/Game.unity       |
| 2     | Assets/Scenes/GameOver.unity   |

---

## Tag & Layer Setup

The project's `TagManager.asset` already defines the required tags and layers.
If they don't appear automatically, create them manually:

### Tags:
- `Player`
- `PlayerBullet`
- `EnemyBullet`
- `PowerUp`
- `Enemy`
- `Background`
- `Boundary`

### Layers (user layers starting at index 8):
- Layer 8: `Player`
- Layer 9: `Enemy`
- Layer 10: `PlayerBullet`
- Layer 11: `EnemyBullet`
- Layer 12: `PowerUp`
- Layer 13: `Background`
- Layer 14: `Boundary`

### Sorting Layers:
- `Background`
- `Midground`
- `Foreground`
- `Player`
- `Effects`
- `UI`

---

## Audio Setup (Optional)

Place audio files in `Assets/Resources/Audio/`:

```
Assets/
  Resources/
    Audio/
      Music/
        menu_music.wav    (or .mp3, .ogg)
        game_music.wav
      SFX/
        player_shoot.wav
        enemy_shoot.wav
        explosion.wav
        powerup.wav
        hit.wav
        shield_up.wav
        shield_hit.wav
        weapon_upgrade.wav
        ui_click.wav
        wave_start.wav
        game_over.wav
        combo_up.wav
```

The AudioManager loads clips from Resources at runtime. Missing clips are
silently skipped — the game works perfectly without audio files.
