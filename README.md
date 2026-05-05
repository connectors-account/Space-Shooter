# Unity Space Shooter (Windows Desktop)

A complete 2D space-shooter project using Unity + C#. The game is intentionally simple in art style (geometric sprites), but polished in gameplay and structure:

- Player spaceship movement + shooting
- Enemy waves with progressive difficulty
- Multiple enemy types and bullet patterns
- Player health, score, wave system
- Power-ups (weapon, health, shield)
- Parallax scrolling background
- Main menu, options, pause menu, game over UI
- Object pooling for enemies, bullets, and effects
- Runtime-generated placeholder sound effects + music

---

### Unity Version

- **Recommended:** Unity **2022.3.30f1 LTS** (or close 2022.3 LTS version)

---

### Project Structure

```text
space_shooter_game/
├── Assets/
│   ├── Audio/
│   ├── Prefabs/
│   ├── Scenes/
│   ├── Scripts/
│   │   ├── Audio/
│   │   ├── Core/
│   │   ├── Enemies/
│   │   ├── Player/
│   │   ├── PowerUps/
│   │   ├── Projectiles/
│   │   ├── UI/
│   │   └── Visual/
│   └── Sprites/
├── Packages/
├── ProjectSettings/
└── README.md
```

---

### How to Open the Project in Unity

1. Launch **Unity Hub**.
2. Click **Add project from disk**.
3. Select:
   - `/home/ubuntu/space_shooter_game/`
4. Open the project with Unity 2022.3 LTS.
5. Create/open a scene:
   - If no scene exists, create a new empty 2D scene.
   - Save it as `Assets/Scenes/Main.unity`.
6. Press **Play**.

> The gameplay scene is auto-built at runtime by `GameBootstrapper` (camera, player, managers, UI, background, pooling setup).

---

### Controls

- **Move:** Arrow keys or WASD
- **Shoot:** Space or Ctrl
- **Pause:** Esc

---

### Build as Windows Executable (.exe)

1. In Unity: **File → Build Settings**
2. Platform: select **Windows, Mac, Linux Standalone**
3. Target Platform: **Windows**
4. Architecture: **x86_64**
5. Click **Add Open Scenes** (ensure `Main.unity` is in list)
6. Click **Build**
7. Choose an output folder (example: `Builds/Windows/`)
8. Unity will generate:
   - `SpaceShooter.exe`
   - `SpaceShooter_Data/`

Run the `.exe` to play.

---

### System Requirements (for built game)

- OS: Windows 10/11 (64-bit)
- CPU: Dual-core 2.0 GHz+
- RAM: 4 GB minimum
- GPU: DirectX 10 capable
- Disk: ~250 MB free space

---

### Script Highlights

- `GameBootstrapper.cs` – full runtime scene + object setup
- `GameManager.cs` – game states, score, wave, pause, game over
- `WaveManager.cs` – wave progression + scaling spawn difficulty
- `ObjectPoolManager.cs` – pooled bullets/enemies/effects/power-ups
- `PlayerController.cs` / `PlayerHealth.cs` – movement, shooting, upgrades, shield
- `EnemyController.cs` – multiple enemy behaviors + fire patterns
- `PowerUp.cs` – weapon/health/shield pickups
- `UIManager.cs` – main menu/options/pause/game over/HUD
- `ParallaxScroller.cs` – layered scrolling background
- `EffectManager.cs` – hit/explosion visual pulses
- `SoundManager.cs` – placeholder synthesized SFX/music manager

---

### Replacing Placeholder Assets

- Sprites: put custom sprite assets under `Assets/Sprites/`
- Audio: put clips under `Assets/Audio/`
- Then update references in `GameBootstrapper` / `SoundManager`

---

### Notes

- No TODOs or placeholder logic paths remain in gameplay scripts.
- Gameplay constants are centralized in `GameConfig`.
- Object pooling is used for high-frequency spawned objects.
