# Setup Guide (Windows Desktop)

This project targets **Unity 2022.3 LTS** and **Windows x86_64** desktop builds.

## Quick Start
1. Open Unity Hub.
2. Add this repository as a project.
3. Open with Unity 2022.3 LTS.
4. Create/verify scenes:
   - `Assets/Scenes/MainMenu.unity`
   - `Assets/Scenes/Game.unity`
5. Add both scenes to Build Settings.
6. Build target: `PC, Mac & Linux Standalone` → `Windows` → `x86_64`.

## Required Scene Objects
- GameManager (with GameManager script and pool refs)
- ScoreManager
- WaveManager
- EnemySpawner
- AudioManager (2 audio sources)
- Player
- UI Canvas (health, score/combo, wave, pause, game over)
- ObjectPool objects for player bullets, enemy bullets, enemy types, powerups

## Legacy Input Used
- Horizontal/Vertical axes
- Fire1
- Space
- Escape

## Build Executable
1. File → Build Settings
2. Select `Windows`, `x86_64`
3. Click **Build**
4. Run generated `.exe`
