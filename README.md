# 🚀 Space Shooter — Unity 2D Desktop Game

A simple but fully playable 2D space-shooter for Windows (and any Unity-supported desktop).
Fly your ship, blast incoming enemies, survive as long as you can, and rack up a high score.

All gameplay logic is implemented in six clean, fully-commented C# scripts — **no placeholders**.
This README walks you through setting up the Unity project from these scripts, wiring up the
scene, and building a standalone Windows `.exe`.

---

## 🎮 Gameplay

| Action            | Control                          |
|-------------------|----------------------------------|
| Move ship         | **W A S D** or **Arrow keys**    |
| Shoot             | **Space** (hold to auto-fire)    |
| Restart (on death)| **R**                            |

- Enemies spawn at the top and descend using one of three movement patterns (straight, sine-wave, diagonal).
- Player bullets destroy enemies and award points.
- Enemies that collide with the player deal damage.
- When health hits 0 the **Game Over** screen appears with your final score.
- Difficulty ramps up automatically — enemies spawn faster the longer you survive.

---

## 📁 Project Structure

```
space_shooter_unity/
├── Assets/
│   ├── Scripts/
│   │   ├── GameManager.cs      # Game state, score, health, game-over/restart
│   │   ├── PlayerController.cs  # Player movement + shooting
│   │   ├── BulletController.cs  # Bullet movement + collision
│   │   ├── EnemyController.cs   # Enemy stats, movement patterns, death
│   │   ├── EnemySpawner.cs      # Timed enemy spawning + difficulty ramp
│   │   └── UIManager.cs         # Score/health HUD + game-over panel
│   ├── Prefabs/                 # (you create Player, Enemy, Bullet prefabs here)
│   └── Scenes/                  # (you create Main.unity here)
├── Packages/manifest.json       # Required Unity packages
├── ProjectSettings/             # Editor version + tags
├── .gitignore
└── README.md
```

> **Note:** The `Library/`, `Temp/`, and other generated folders are created by Unity
> automatically the first time you open the project. They are intentionally git-ignored.

---

## 🛠️ Part 1 — Set Up the Unity Project

### Requirements
- **Unity Hub** + **Unity Editor 2022.3 LTS** (or any 2021.3+ LTS).
  Other recent versions work fine; the scripts use no version-specific APIs.
- During install, make sure to include the **"Windows Build Support (IL2CPP/Mono)"** module
  (it is included by default when installing on Windows).

### Open the project
1. Open **Unity Hub → Projects → Add → Add project from disk**.
2. Select the `space_shooter_unity` folder (the one containing `Assets/` and `Packages/`).
3. Unity will import the project and generate the `Library/` folder. This may take a minute.

If Unity prompts that the editor version differs from `ProjectSettings/ProjectVersion.txt`,
just open it with your installed version — that's expected and safe.

---

## 🧩 Part 2 — Build the Scene

You'll create a handful of simple GameObjects (using Unity's built-in sprites — no art assets needed)
and attach the provided scripts.

### Step 0 — Create the scene
1. **File → New Scene → Basic 2D (or Empty)**, then **File → Save As** → `Assets/Scenes/Main.unity`.
2. Select the **Main Camera**. In the Inspector set:
   - **Projection: Orthographic**
   - **Size: 6** (a good starting view height)
   - **Background:** a dark color (e.g. near-black) for a "space" look.

### Step 1 — Create the Player
1. **GameObject → 2D Object → Sprites → Square** (or **Capsule**). Rename it **Player**.
2. Set its **Tag** to **Player** (top of Inspector → Tag dropdown → Player).
3. Tint it (Sprite Renderer → Color) e.g. cyan, and scale it down to ~`(0.6, 0.8, 1)`.
4. **Add Component → Rigidbody 2D**. Set **Gravity Scale = 0**.
5. **Add Component → Box Collider 2D** and tick **Is Trigger**.
6. **Add Component → Player Controller** (the `PlayerController.cs` script).
7. Position it near the bottom of the screen, e.g. `(0, -3.5, 0)`.

### Step 2 — Create the Bullet prefab
1. **GameObject → 2D Object → Sprites → Square**. Rename it **Bullet**.
2. Scale it small, e.g. `(0.12, 0.4, 1)`, and color it yellow/white.
3. **Add Component → Rigidbody 2D** → **Gravity Scale = 0**.
4. **Add Component → Box Collider 2D** → tick **Is Trigger**.
5. **Add Component → Bullet Controller** (`BulletController.cs`).
6. Drag **Bullet** from the Hierarchy into the **Assets/Prefabs** folder to make it a prefab,
   then **delete** the Bullet from the Hierarchy (it should only exist as a prefab).
7. Select the **Player** in the Hierarchy and drag the **Bullet prefab** into the
   PlayerController's **Bullet Prefab** field.
   - *(Optional)* Create an empty child of Player named **Muzzle**, place it at the ship's nose,
     and drag it into the **Muzzle** field. If left empty, bullets spawn just above the ship.

### Step 3 — Create the Enemy prefab
1. **GameObject → 2D Object → Sprites → Square**. Rename it **Enemy**.
2. Set its **Tag** to **Enemy** (the tag is already defined in this project; if not, add it via
   **Tag dropdown → Add Tag... → +** and type `Enemy`).
3. Color it red and scale ~`(0.6, 0.6, 1)`.
4. **Add Component → Rigidbody 2D** → **Gravity Scale = 0**.
5. **Add Component → Box Collider 2D** → tick **Is Trigger**.
6. **Add Component → Enemy Controller** (`EnemyController.cs`). Pick a default **Movement Pattern**
   (e.g. Sine) — the spawner can spawn this prefab repeatedly.
7. Drag **Enemy** into **Assets/Prefabs** to make it a prefab, then delete it from the Hierarchy.

### Step 4 — Create the Enemy Spawner
1. **GameObject → Create Empty**. Rename it **EnemySpawner**, position `(0,0,0)`.
2. **Add Component → Enemy Spawner** (`EnemySpawner.cs`).
3. Drag the **Enemy prefab** into the spawner's **Enemy Prefab** field.

### Step 5 — Create the UI (HUD + Game Over)
1. **GameObject → UI → Canvas** (this also creates an EventSystem — keep it).
   - Set the Canvas **Render Mode** to **Screen Space - Overlay**.
2. **Score text:** **GameObject → UI → Text** (Legacy). Rename **ScoreText**.
   Anchor it top-left, set text to `Score: 0`.
3. **Health text:** **GameObject → UI → Text** (Legacy). Rename **HealthText**.
   Anchor it top-right, set text to `Health: 100`.
4. *(Optional health bar):* **GameObject → UI → Image**, set **Image Type = Filled**,
   **Fill Method = Horizontal**. Name it **HealthBarFill**.
5. **Game Over panel:**
   - **GameObject → UI → Panel**, rename **GameOverPanel**. Give it a semi-transparent dark color.
   - Add a child **Text** (Legacy) named **FinalScoreText** (centered, large font).
   - *(Optional)* Add a child **Button** (Legacy) named **RestartButton** with label "Restart".
   - **Disable GameOverPanel** for now (uncheck the box next to its name) — the script shows it on death.

   > **Using TextMeshPro instead of Legacy Text?** The scripts use `UnityEngine.UI.Text`.
   > To use TMP, change the field types in `UIManager.cs` from `Text` to `TMP_Text`
   > and add `using TMPro;`. Legacy Text needs zero extra setup, so it's the default here.

6. **Create the UI Manager object:** select the **Canvas**, then **Add Component → UI Manager**
   (`UIManager.cs`). In the Inspector, drag the matching objects into the fields:
   - **Score Text** ← ScoreText
   - **Health Text** ← HealthText
   - **Health Bar Fill** ← HealthBarFill *(optional)*
   - **Game Over Panel** ← GameOverPanel
   - **Final Score Text** ← FinalScoreText
   - **Restart Button** ← RestartButton *(optional)*

### Step 6 — Create the Game Manager
1. **GameObject → Create Empty**. Rename it **GameManager**.
2. **Add Component → Game Manager** (`GameManager.cs`).
3. Drag the **Canvas** (which has the UIManager) into the GameManager's **UI Manager** field.
   *(If you skip this, GameManager auto-finds the UIManager at runtime.)*

### Step 7 — Press Play! ▶️
Hit the **Play** button. You should be able to move, shoot, destroy enemies, take damage,
and see the Game Over screen when health reaches 0. Press **R** (or the Restart button) to play again.

---

## 🪟 Part 3 — Build the Windows Executable

1. **File → Build Settings…**
2. Under **Scenes In Build**, click **Add Open Scenes** (make sure `Main` is listed and ticked).
3. Select **Platform: Windows, Mac, Linux**, then set **Target Platform: Windows** and
   **Architecture: x86_64**. Click **Switch Platform** if it isn't already selected.
4. *(Optional)* **Player Settings… → Player** to set the product name, company, icon, and resolution.
5. Click **Build** (or **Build And Run**).
6. Choose an output folder (e.g. `Builds/Windows`).
7. Unity produces:
   - `SpaceShooter.exe` (your game)
   - a `SpaceShooter_Data/` folder
   - `UnityPlayer.dll` and other runtime files
8. **To distribute:** zip the **entire output folder** (the `.exe` needs the `_Data` folder and
   DLLs beside it). Double-click `SpaceShooter.exe` to play. No Unity install required on the target PC.

> **Tip:** For a single distributable file, zip the whole build folder. The `.exe` alone will not run.

---

## ⚙️ Tweaking the Game (all in the Inspector)

Every script exposes tunable values — no code changes needed:

- **PlayerController:** `Move Speed`, `Fire Rate`, `Collision Damage`, `Edge Padding`.
- **BulletController:** `Speed`, `Lifetime`, `Damage`.
- **EnemyController:** `Max Health`, `Score Value`, `Speed`, `Movement Pattern`, sine amplitude/frequency.
- **EnemySpawner:** `Start Interval`, `Min Interval`, `Difficulty Ramp`, spawn margins.
- **GameManager:** `Starting Health`.

---

## 🧠 How the Scripts Fit Together

- **GameManager** (singleton) holds score & health and broadcasts UI updates. It freezes time on
  game over and reloads the scene on restart.
- **PlayerController** reads input, moves with physics (clamped to screen), and spawns **player bullets**.
- **BulletController** moves a bullet, self-destructs off-screen/after a lifetime, and on trigger hit
  damages an enemy (player bullet) — the same component can be reused for enemy bullets.
- **EnemyController** moves via a chosen pattern, takes damage, awards score on death, and cleans up
  when it leaves the screen.
- **EnemySpawner** instantiates enemies on a shrinking timer for escalating difficulty.
- **UIManager** updates the score/health HUD and shows/hides the game-over panel.

Enjoy, and feel free to extend it with power-ups, enemy bullets, sound effects, and sprites! 🛸
