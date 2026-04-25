# Space-Shooter

## Unity Space Shooter (Windows Desktop)

A complete, simple 2D space-shooter Unity project with:
- Player movement (WASD / Arrow Keys)
- Shooting (Space)
- Enemy wave progression with increasing difficulty
- Multiple enemy types and bullet patterns
- Collision, health, score, and power-ups
- Parallax star background
- Main Menu, Pause Menu, and Game Over UI
- Procedural placeholder SFX

Project path:
`/home/ubuntu/space_shooter_game/`

---

### Unity Version
This project is configured for **Unity 2022.3.20f1 (LTS)**.

If you open in a nearby 2022.3 LTS patch version, Unity should upgrade it automatically.

---

### Project Structure

- `Assets/Scenes/`
  - `MainMenu.unity`
  - `GamePlay.unity`
- `Assets/Scripts/`
  - `Core/` (bootstrap + game state)
  - `Player/`
  - `Enemy/`
  - `Combat/`
  - `PowerUps/`
  - `Environment/`
  - `UI/`
  - `Audio/`
  - `Utils/`
- `Assets/Prefabs/`
  - Player, Enemy, Bullet, PowerUp prefabs (simple reusable prefabs)
- `Assets/Sprites/`
  - Simple geometric sprite placeholders (PNG)
- `Assets/Audio/`
  - Placeholder instructions for replacing SFX
- `ProjectSettings/`
  - Project/build settings files
- `Packages/`
  - Unity package manifest

---

### Open the Project in Unity

1. Open **Unity Hub**.
2. Click **Open**.
3. Select folder: `/home/ubuntu/space_shooter_game`.
4. Ensure Unity Hub opens with **2022.3.x LTS**.
5. Let Unity import assets and compile scripts.

---

### Build for Windows (Standalone EXE)

1. In Unity, go to **File → Build Settings**.
2. Confirm scenes are included:
   - `Assets/Scenes/MainMenu.unity`
   - `Assets/Scenes/GamePlay.unity`
3. Select **PC, Mac & Linux Standalone**.
4. Set target platform to **Windows**.
5. Architecture: **x86_64**.
6. Click **Build** (or **Build and Run**).
7. Choose output folder (for example, `Builds/Windows`).

Unity will generate a `.exe` and supporting files.

---

### How to Play

- **Move**: `WASD` or `Arrow Keys`
- **Shoot**: `Space`
- **Pause/Resume**: `Esc`

Gameplay loop:
1. Start in Main Menu.
2. Click **Start Game**.
3. Survive waves of enemies.
4. Collect power-ups:
   - **Shield**: temporary invulnerability
   - **Rapid Fire**: faster firing
   - **Health Restore**: +1 health
5. If health reaches 0, Game Over menu appears.
6. Restart or return to Main Menu.

---

### Notes on Audio Placeholders

Current sound effects are generated procedurally in code (`AudioManager.cs`) so the game works immediately without external audio files.

To use custom audio:
1. Add clips in `Assets/Audio/`
2. Update `AudioManager.cs` to reference your clips.

---

### Important Implementation Detail

This project is intentionally code-driven and lightweight:
- Scenes are minimal.
- Runtime bootstrap scripts create and wire gameplay systems/UI automatically.
- Functionality is complete and focused on mechanics over complex visuals.

---

### Troubleshooting

- If scripts fail to compile on first import, wait for package restore to complete.
- If input does not respond, check **Project Settings → Player → Active Input Handling** and set to **Both** or **Input Manager (Old)**.
- If scenes are not listed in Build Settings, add both scene files manually.
