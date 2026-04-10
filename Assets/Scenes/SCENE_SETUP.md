# Scene Setup

Create two scenes in `Assets/Scenes/`:

1. `MainMenu.unity`
   - Optional dedicated menu scene. You can also keep menu UI in the game scene.

2. `GameScene.unity`
   - Main playable scene.
   - Required root objects:
     - `Main Camera`
     - `GameManager` (with `SpaceShooter.Managers.GameManager`)
     - `SpawnManager` (with `SpaceShooter.Managers.SpawnManager`)
     - `InputHandler` (with `SpaceShooter.InputSystem.InputHandler`)
     - `Player`
     - `UICanvas` (with UI panels)
     - `ParallaxBackground` (optional but recommended)
     - `StarField` (optional but recommended)
