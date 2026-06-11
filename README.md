# Space Shooter — Unity (C#)

A simple but **fully functional** 2D space-shooter for Windows desktop, built with Unity and C#.
You fly a ship, shoot incoming enemies, survive escalating waves, and chase a high score.

- Player ship: WASD / Arrow keys to move, **Space** to shoot
- Enemies spawn in **waves** that grow larger and faster
- Enemies weave, shoot back, and damage you on contact
- Health system for player and enemies
- Score (scaled by wave) + persistent high score
- Full game loop: **Menu → Playing → Game Over → Restart**
- HUD: score, wave, health bar; plus Start and Game Over screens

---

## 1. Requirements

- **Unity 2022.3 LTS** (the project was authored against `2022.3.20f1`; any 2022.3.x works).
  Install via [Unity Hub](https://unity.com/download).
- During install, include the **"Windows Build Support (IL2CPP/Mono)"** module so you can build the `.exe`.
- No external art/audio assets are required — visuals are generated at runtime.

---

## 2. Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── GameManager.cs        # Game state, score, waves (singleton)
│   │   ├── Health.cs             # Reusable health for player & enemies
│   │   ├── PlayerController.cs   # Movement + shooting
│   │   ├── EnemyController.cs    # Enemy AI, weaving, shooting, contact damage
│   │   ├── BulletController.cs   # Projectile movement + collision/damage
│   │   ├── SpawnManager.cs       # Wave spawning & difficulty scaling
│   │   ├── UIManager.cs          # HUD + menu/game-over panels
│   │   └── Bootstrap/
│   │       └── GameBootstrap.cs  # Builds the whole playable scene at runtime
│   ├── Editor/
│   │   └── BuildScript.cs        # One-click "Build → Build Windows (x64)" menu
│   ├── Prefabs/                  # (for the manual setup path)
│   ├── Scenes/                   # MainScene.unity lives here once you save it
│   └── Materials/
├── Packages/manifest.json        # Package dependencies (2D, uGUI, physics2D)
├── ProjectSettings/              # Unity project settings (version, tags, build list)
└── README.md
```

---

## 3. Open the Project in Unity

1. Open **Unity Hub → Add → Add project from disk** and select the `space_shooter_game` folder.
2. Open it with **Unity 2022.3.x**. Unity will import and generate the `Library/` folder (first import takes a minute).
3. If prompted about the editor version, choosing any **2022.3.x** is fine.

> **Tags:** the project ships with the custom tags `Enemy` and `Boundary` (in `ProjectSettings/TagManager.asset`).
> `Player` and `MainCamera` are Unity built-ins. If a tag ever appears missing, add it via **Edit → Project Settings → Tags and Layers**.

---

## 4. Fastest Way to Play (Auto-Built Scene) — Recommended

The included `GameBootstrap` script constructs the **entire game at runtime** (camera, player, enemies, bullets, managers, and a complete UI) using generated sprites. No manual prefab wiring needed.

1. In Unity, go to **File → New Scene** → choose **Basic (Built-in)** or an empty scene.
2. In the Hierarchy: **right-click → Create Empty**. Name it `Bootstrap`.
3. Select it, then in the Inspector click **Add Component** and add **`Game Bootstrap`**.
4. **File → Save As** and save the scene to **`Assets/Scenes/MainScene.unity`**
   (this is the path already registered in Build Settings).
5. Press **Play** ▶. The menu appears — press **Enter** or click **START**.

That's it — a fully playable game.

---

## 5. Manual Scene & Prefab Setup (Optional, for full control)

If you prefer to build the scene by hand with your own art:

### a) Bullet prefab
1. Create a 2D sprite: **GameObject → 2D Object → Sprites → Square**. Name it `Bullet`.
2. Add components: **Rigidbody 2D** (Gravity Scale = 0, Body Type = Kinematic), **Box Collider 2D** (check *Is Trigger*), and **`Bullet Controller`**.
3. Drag it into **Assets/Prefabs/** to make a prefab, then delete it from the scene.

### b) Enemy prefab
1. Create a Square sprite named `Enemy`. Set its **Tag = Enemy**.
2. Add: **Rigidbody 2D** (Gravity 0, Kinematic), **Box Collider 2D** (*Is Trigger*), **`Health`** (Max Health e.g. 50), and **`Enemy Controller`**.
3. On `Enemy Controller`, assign the **Bullet** prefab to `Bullet Prefab`.
4. Drag into **Assets/Prefabs/**, then delete from scene.

### c) Player
1. Create a Square sprite named `Player`. Set its **Tag = Player**.
2. Add: **Rigidbody 2D** (Gravity 0, Kinematic), **Box Collider 2D** (*Is Trigger*), **`Health`** (Max Health 100, *Is Player* = true), and **`Player Controller`**.
3. Create a child empty `Muzzle` at the ship's nose; assign it to `Player Controller → Muzzle`.
4. Assign the **Bullet** prefab to `Player Controller → Bullet Prefab`.
5. Place the player near the bottom of the screen.

### d) Managers
1. Create an empty `GameManager` and add the **`Game Manager`** component.
2. Create an empty `SpawnManager`, add **`Spawn Manager`**, and assign your **Enemy** prefab(s) to its `Enemy Prefabs` array.

### e) Camera & Boundary
1. Set **Main Camera** to **Orthographic**, Size ≈ 6.
2. (Optional) Add a `Boundary` GameObject with an **Edge/Box Collider 2D** (*Is Trigger*), tagged **Boundary**, ringing the screen so off-screen bullets despawn.

### f) UI (uGUI)
1. **GameObject → UI → Canvas** (Screen Space – Overlay). It auto-adds an EventSystem.
2. Add `Text` elements for **Score** and **Wave**, a **Slider** for the **Health bar**, and three panels: **Menu**, **HUD**, **GameOver** (with `Final Score` and `High Score` texts + Start/Restart **Buttons**).
3. Add the **`UI Manager`** component to the Canvas and drag every element into its matching Inspector slot.
4. Wire the Start button's `OnClick` → `UIManager.OnStartButton`, and the Restart button → `UIManager.OnRestartButton`.

### g) Save the scene
Save as **`Assets/Scenes/MainScene.unity`**.

---

## 6. Controls

| Action      | Key                         |
|-------------|-----------------------------|
| Move        | **W A S D** or **Arrows**   |
| Shoot       | **Space** (hold to auto-fire) |
| Start game  | **Enter** / Start button    |
| Restart     | **Enter** / Restart button  |

---

## 7. Build a Windows Executable (.exe)

### Option A — One click (uses the included build script)
1. Make sure your scene is saved at **`Assets/Scenes/MainScene.unity`** and is enabled in
   **File → Build Settings → Scenes In Build** (click **Add Open Scenes** if the list is empty).
2. In the Unity menu bar, choose **Build → Build Windows (x64)**.
3. The game is built to **`Builds/Windows/SpaceShooter.exe`**. Run that `.exe` on any Windows PC.

### Option B — Standard Build Settings dialog
1. **File → Build Settings**.
2. Select **Windows** platform → set **Target Platform = Windows**, **Architecture = x86_64**.
   (Click **Switch Platform** if it is not already selected.)
3. Click **Add Open Scenes** so `MainScene` is listed and checked.
4. Click **Build**, choose an output folder (e.g. `Builds/Windows`), and Unity produces `SpaceShooter.exe`
   plus a `_Data` folder. **Distribute the whole folder together.**

> **Command-line / CI build (optional):**
> ```
> "C:\Program Files\Unity\Hub\Editor\2022.3.20f1\Editor\Unity.exe" -batchmode -quit \
>   -projectPath "<path>\space_shooter_game" -executeMethod BuildScript.BuildWindows
> ```

---

## 8. Tuning the Game

Most behaviour is exposed in the Inspector (or set by `GameBootstrap`):

- **PlayerController:** `moveSpeed`, `fireRate`, `bulletDamage`, `bulletSpeed`
- **EnemyController:** `moveSpeed`, `weaveAmplitude`, `fireInterval`, `contactDamage`
- **SpawnManager:** `baseEnemiesPerWave`, `enemiesIncrementPerWave`, `spawnInterval`, `minSpawnInterval`, `timeBetweenWaves`
- **Health:** `maxHealth`, `invulnerabilityTime`
- **GameManager:** `pointsPerEnemy` (final score = points × current wave)

---

## 9. Notes

- The runtime-generated visuals are plain colored shapes so the project runs with **zero external assets**.
  Swap in your own sprites any time via the manual setup path.
- High score is stored with `PlayerPrefs` and persists between runs on the same machine.
- All scripts are complete and self-contained — no placeholders or TODOs.
