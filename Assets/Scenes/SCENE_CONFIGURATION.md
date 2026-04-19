# Scene Configuration

## Main Scene
- File path: `Assets/Scenes/Main.unity`
- The runtime bootstrap script (`GameBootstrap`) auto-initializes game systems before scene content loads.
- If `Main.unity` does not exist yet, create it with:
  - Unity menu: **Tools → Space Shooter → Create Main Scene**
  - This creates an empty playable scene and adds it to Build Settings.

## Runtime-created scene objects
When Play Mode starts, the project creates:
- Main camera (orthographic)
- Parallax background layers
- AudioManager
- SpawnManager
- GameManager
- Full Canvas UI (main menu, HUD, pause, game-over)
- Player object (spawned when game starts)

No manual scene wiring is required.
