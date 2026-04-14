# Space Shooter - Setup and Build Guide (Unity 2022.3 LTS)

This guide is the full step-by-step process to open, configure, and build the project as a Windows executable.

## 1) Set up the Unity project

1. Install **Unity Hub**.
2. Install **Unity 2022.3 LTS** editor (2D module + Windows Build Support/IL2CPP).
3. In Unity Hub, click **Open** and select this project folder.
4. Wait for import and script compilation.
5. Confirm project uses **Built-in Render Pipeline**.

---

## 2) Import scripts and assets

All code and placeholder assets are already in the repo:

- Scripts: `Assets/Scripts/`
- Sprites: `Assets/Sprites/`
- Audio: `Assets/Audio/`

### Required Tags
Go to **Edit > Project Settings > Tags and Layers** and ensure these tags exist:
- `Player`
- `Enemy`
- `PlayerBullet`
- `EnemyBullet`
- `PowerUp`

### Input
This project uses Unity classic input axes:
- `Horizontal`
- `Vertical`
And key polling:
- `Space` (shoot)
- `Escape` (pause)

---

## 3) Configure scenes and prefabs

### Scene configuration
Use:
- `Assets/Scenes/SCENE_CONFIGURATION.md`

Create two scenes in Unity:
- `MainMenuScene.unity` (optional split menu scene)
- `GameScene.unity` (main gameplay scene)

Add at minimum in gameplay scene:
- Main Camera (+ `ScreenBounds` script)
- `GameManager` object (+ `GameManager` script)
- `AudioManager` object (+ `AudioManager` script + audio clips)
- `SpawnManager` object (+ `SpawnManager` script + enemy prefab refs)
- `Player` object (+ `PlayerController` script)
- `UICanvas` + `UIManager` object (+ `UIManager` script)
- `StarField` and `ParallaxBackground`

### Prefab configuration
Use:
- `Assets/Prefabs/PREFAB_SETUP.md`

Create and assign these prefabs:
- PlayerBullet
- EnemyBullet
- EnemyBasic
- EnemyZigzag
- EnemyTank
- PowerUpHealth
- PowerUpRapidFire
- PowerUpShield

Wire references exactly as documented in prefab setup.

---

## 4) Build game as Windows .exe

1. Open **File > Build Settings**.
2. Select **PC, Mac & Linux Standalone**.
3. Target Platform: **Windows**.
4. Architecture: **x86_64**.
5. Add gameplay scene(s) to **Scenes In Build**.
6. Open **Player Settings** and set:
   - Product Name: `Space Shooter`
   - Default resolution: `1024 x 768` (or preferred)
   - Fullscreen mode: Windowed
7. Click **Build**.
8. Choose output folder (for example `Builds/Windows`).
9. Run generated `Space Shooter.exe`.

---

## Sanity checklist before building

- No compile errors in Console.
- All script references assigned in Inspector.
- All required tags exist.
- SpawnManager has all enemy prefab references.
- Player has bullet prefab + fire point.
- Enemy prefabs have enemy bullet prefab + fire point.
- UIManager panel and text/button references are connected.
- Audio clips assigned in AudioManager.

---

## Note
If you copy this project to another machine, open with Unity 2022.3 LTS and let Unity reimport all assets before building.
