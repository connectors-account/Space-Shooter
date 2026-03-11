# 🚀 Unity Space Shooter – Complete Setup & Build Guide

A simple but fully functional 2D space-shooter game for **Windows desktop** built with Unity.

---

## 📁 Project Structure

```
unity_space_shooter/
├── Assets/
│   ├── Scripts/
│   │   ├── PlayerController.cs     # Player movement, shooting, health, collision
│   │   ├── BulletController.cs     # Bullet movement and enemy-hit detection
│   │   ├── EnemyController.cs      # Enemy movement (fall + sway)
│   │   ├── EnemySpawner.cs         # Spawns enemies with increasing difficulty
│   │   ├── GameManager.cs          # Score tracking, game state, restart/quit
│   │   ├── UIManager.cs            # HUD (score, health) and Game Over panel
│   │   ├── BackgroundScroller.cs   # Scrolling space background (option A)
│   │   └── StarfieldGenerator.cs   # Procedural star particles (option B)
│   ├── Scenes/
│   ├── Prefabs/
│   ├── Materials/
│   └── Sprites/
└── README.md                       # ← You are here
```

---

## 🛠️ Prerequisites

| Requirement | Recommended Version |
|---|---|
| Unity Editor | **2021.3 LTS** or newer (any 2022/2023/6000 works too) |
| Build target | **Windows, Mac & Linux Standalone** (installed via Unity Hub → Add Modules) |
| IDE | Visual Studio or VS Code with C# extension |

---

## 📝 Step-by-Step Setup Instructions

### STEP 1 — Create a New Unity Project

1. Open **Unity Hub** → click **New Project**.
2. Select the **2D (Built-in Render Pipeline)** template.  
   *(If using URP, choose 2D (URP) — the scripts still work, but shaders differ.)*
3. Name the project `SpaceShooter` and choose a save location.
4. Click **Create project**.

### STEP 2 — Import the Scripts

1. In the Unity **Project** panel, you already have an `Assets` folder.
2. Copy **all 8 `.cs` files** from `Assets/Scripts/` into your Unity project's `Assets/Scripts/` folder.
   - You can drag-and-drop them into the Project panel, or copy them into the folder on disk.
3. Unity will auto-compile them. Fix any errors if your Unity version is very old (unlikely).

### STEP 3 — Create Tags

Several scripts rely on tags for collision detection. Create them:

1. Go to **Edit → Project Settings → Tags and Layers**.
2. Add the following **Tags** (if they don't already exist):
   - `Player`
   - `Enemy`
   - `PlayerBullet`

### STEP 4 — Create the Player Ship

1. In the **Hierarchy**, right-click → **2D Object → Sprites → Triangle** (or Square / Circle).
   - Rename it to **`Player`**.
   - Set the **Tag** to `Player`.
   - Set **Position** to `(0, -3.5, 0)`.
   - Set **Scale** to `(0.6, 0.8, 1)` (adjust to taste).
   - Set the **Sprite Renderer → Color** to a bright green or blue.
2. Add a **Rigidbody2D** component:
   - Set **Body Type** to `Kinematic` (we move via script, not physics).
   - Set **Gravity Scale** to `0`.
3. Add a **BoxCollider2D** (or PolygonCollider2D) component:
   - Check **Is Trigger** = ✅.
4. Add the **PlayerController** script component (drag it on, or Add Component → search).
5. Create a child empty object for the fire point:
   - Right-click `Player` → **Create Empty**, rename to **`FirePoint`**.
   - Set its **Local Position** to `(0, 0.6, 0)` (just above the top of the ship sprite).
6. In the PlayerController inspector, assign **Fire Point** to this child object.
   - Leave **Bullet Prefab** empty for now (we'll set it after creating the prefab).

### STEP 5 — Create the Bullet Prefab

1. In the Hierarchy, right-click → **2D Object → Sprites → Circle** (or Capsule).
   - Rename it to **`Bullet`**.
   - Set the **Tag** to `PlayerBullet`.
   - Set **Scale** to `(0.15, 0.3, 1)`.
   - Set the **Sprite Renderer → Color** to yellow.
2. Add a **Rigidbody2D** component:
   - **Body Type** = `Kinematic`.
   - **Gravity Scale** = `0`.
3. Add a **BoxCollider2D** component:
   - Check **Is Trigger** = ✅.
4. Add the **BulletController** script component.
5. **Drag the Bullet from the Hierarchy into the `Assets/Prefabs` folder** to create a prefab.
6. **Delete** the Bullet from the Hierarchy (the prefab remains in Assets).
7. Now select the **Player** object, find the **PlayerController** component, and drag the **Bullet prefab** into the **Bullet Prefab** slot.

### STEP 6 — Create the Enemy Prefab

1. In the Hierarchy, right-click → **2D Object → Sprites → Diamond** (or Hexagon / Square).
   - Rename it to **`Enemy`**.
   - Set the **Tag** to `Enemy`.
   - Set **Scale** to `(0.7, 0.7, 1)`.
   - Set the **Sprite Renderer → Color** to red.
2. Add a **Rigidbody2D** component:
   - **Body Type** = `Kinematic`.
   - **Gravity Scale** = `0`.
3. Add a **BoxCollider2D** (or CircleCollider2D) component:
   - Check **Is Trigger** = ✅.
4. Add the **EnemyController** script component.
5. **Drag Enemy from Hierarchy into `Assets/Prefabs`** to create the prefab.
6. **Delete** the Enemy from the Hierarchy.

### STEP 7 — Create the Enemy Spawner

1. Right-click in Hierarchy → **Create Empty**, rename to **`EnemySpawner`**.
   - Position doesn't matter (it spawns enemies at the Y you set in the Inspector).
2. Add the **EnemySpawner** script component.
3. Drag the **Enemy prefab** from `Assets/Prefabs` into the **Enemy Prefab** slot.

### STEP 8 — Create the Game Manager

1. Right-click in Hierarchy → **Create Empty**, rename to **`GameManager`**.
2. Add the **GameManager** script component.

### STEP 9 — Create the UI (Canvas)

1. Right-click in Hierarchy → **UI → Canvas**.
   - Set **Canvas Scaler → UI Scale Mode** to **Scale With Screen Size**.
   - Set **Reference Resolution** to `1920 × 1080`.

2. **Score Text:**
   - Right-click Canvas → **UI → Text** (or **Text – TextMeshPro** if you prefer TMP).
   - Rename to **`ScoreText`**.
   - **Anchor**: Top-Left.
   - **Position**: `(120, -30, 0)` (or use anchor presets).
   - **Text**: `Score: 0`
   - **Font Size**: `32`.
   - **Color**: White.

3. **Health Text:**
   - Right-click Canvas → **UI → Text**.
   - Rename to **`HealthText`**.
   - **Anchor**: Top-Right.
   - **Position**: `(-140, -30, 0)`.
   - **Text**: `Health: 5 / 5`
   - **Font Size**: `32`.
   - **Color**: White.

4. **Health Bar (Optional):**
   - Right-click Canvas → **UI → Image** (background bar – dark grey).
     - Anchor: Top-Right, size `(200, 20)`.
   - Child **UI → Image** (fill bar – green).
     - Set **Image Type** to `Filled`, **Fill Method** = Horizontal, **Fill Origin** = Left.
     - Rename to **`HealthBarFill`**.

5. **Game Over Panel:**
   - Right-click Canvas → **UI → Panel**.
   - Rename to **`GameOverPanel`**.
   - Set **Image → Color** to semi-transparent black `(0, 0, 0, 180)`.
   - Add children:
     - **UI → Text** named **`GameOverTitle`** — text: `GAME OVER`, font size 72, white, centred.
     - **UI → Text** named **`FinalScoreText`** — text: `Final Score: 0`, font size 40, white, centred.
     - **UI → Text** named **`RestartText`** — text: `Press R to Restart`, font size 28, white, centred.
   - **Disable** the GameOverPanel by unchecking the checkbox at the top of the Inspector (it should be hidden at start).

6. **Attach UIManager:**
   - Select the **Canvas** object.
   - Add the **UIManager** script component.
   - Drag and assign:
     - `ScoreText` → **Score Text**
     - `HealthText` → **Health Text**
     - `HealthBarFill` → **Health Bar Fill** (optional)
     - `GameOverPanel` → **Game Over Panel**
     - `FinalScoreText` → **Final Score Text**
     - `RestartText` → **Restart Text**

### STEP 10 — Add Background (Choose One)

#### Option A: Scrolling Background Texture

1. Right-click Hierarchy → **3D Object → Quad**.
   - Rename to **`Background`**.
   - Position: `(0, 0, 1)` (behind everything).
   - Scale: `(20, 20, 1)`.
2. Create a material:
   - Right-click `Assets/Materials` → **Create → Material**.
   - Name it `SpaceBackground`.
   - Set Shader to **Unlit/Texture**.
   - Assign any dark starry texture (or a solid dark-blue colour).
   - Set **Tiling** to `(1, 2)` for repetition.
3. Drag the material onto the Quad.
4. Add the **BackgroundScroller** script to the Quad.

#### Option B: Procedural Starfield (No Textures Needed)

1. Right-click Hierarchy → **Create Empty**, rename to **`Starfield`**.
2. Add the **StarfieldGenerator** script. It auto-creates particles at runtime.
3. Make sure **Main Camera → Background colour** is set to **black** `(0,0,0)`.

### STEP 11 — Set Up the Camera

1. Select **Main Camera**.
2. Set **Projection** to `Orthographic`.
3. Set **Size** to `5` (standard for a phone-style view) or `6` for more space.
4. Set **Background** to solid black `(0, 0, 0)`.
5. Position: `(0, 0, -10)`.

### STEP 12 — Configure Physics (Important!)

1. Go to **Edit → Project Settings → Physics 2D**.
2. Open the **Layer Collision Matrix** and make sure relevant layers can interact.
   - For this simple setup using triggers and tags, the default is fine.

---

## 🎮 Controls

| Key | Action |
|---|---|
| W / ↑ | Move up |
| S / ↓ | Move down |
| A / ← | Move left |
| D / → | Move right |
| Space | Shoot |
| R | Restart (after Game Over) |
| Escape | Quit game |

---

## 🔨 Building for Windows Desktop

### 1. Open Build Settings

- Go to **File → Build Settings** (or press `Ctrl+Shift+B`).

### 2. Select Platform

- Click **Windows, Mac & Linux Standalone** in the platform list.
- Set **Target Platform** to **Windows**.
- Set **Architecture** to **x86_64** (64-bit).
- Click **Switch Platform** if it isn't already selected.

### 3. Add Scene to Build

- Click **Add Open Scenes** to add your current scene (e.g., `SampleScene`).
- Make sure it appears in the list with index `0`.

### 4. Player Settings (Optional but Recommended)

Click **Player Settings** and configure:

| Setting | Recommended Value |
|---|---|
| Company Name | Your name or studio |
| Product Name | `Space Shooter` |
| Default Screen Width | `1920` |
| Default Screen Height | `1080` |
| Fullscreen Mode | `Windowed` (for testing) or `Fullscreen Window` |
| Run In Background | ✅ (checked) |
| Default Icon | Assign a custom icon if desired |
| API Compatibility Level | `.NET Standard 2.1` |

### 5. Build

1. Click **Build** (or **Build And Run** to play immediately).
2. Choose / create a folder (e.g., `Build/Windows`).
3. Unity will compile and produce:
   ```
   Build/Windows/
   ├── SpaceShooter.exe            ← Double-click to play!
   ├── SpaceShooter_Data/          ← Game data (must stay with the .exe)
   ├── UnityPlayer.dll
   └── MonoBleedingEdge/
   ```
4. **To distribute:** Zip the entire `Build/Windows` folder and share it. The recipient just unzips and runs the `.exe`.

---

## 🐛 Troubleshooting

| Problem | Solution |
|---|---|
| Bullets don't hit enemies | Make sure both have **Collider2D** with **Is Trigger ✅** and at least one has a **Rigidbody2D**. Check Tags are `Enemy` and `PlayerBullet`. |
| Enemies pass through player | Same as above — check Player has tag `Player`, enemies have tag `Enemy`, and colliders are triggers. |
| Score doesn't update | Ensure UIManager is on the Canvas and the **Score Text** field is assigned in the Inspector. |
| Game Over doesn't show | Make sure **GameOverPanel** is assigned in UIManager and the panel starts disabled. |
| No background / stars | For Option B, ensure the Camera background is black. For Option A, check the Quad is at Z=1 (behind sprites at Z=0). |
| Build fails | Check **File → Build Settings** has your scene listed. Ensure the Windows build module is installed in Unity Hub. |

---

## ✨ Optional Enhancements

Once the base game works, try adding:

- **Sound effects** — `AudioSource.PlayClipAtPoint()` for shooting and explosions
- **Explosions** — Particle effects on enemy death
- **Power-ups** — Speed boost, multi-shot, shields
- **Multiple enemy types** — Different speeds, health, movement patterns
- **High-score persistence** — `PlayerPrefs.SetInt("HighScore", score)`
- **Animated sprites** — Use Unity's Animator with sprite sheets
- **Screen shake** — Small camera shake on player damage for juice

---

## 📄 Script Reference

| Script | Attach To | Purpose |
|---|---|---|
| `PlayerController` | Player GameObject | Movement, shooting, health, collision |
| `BulletController` | Bullet Prefab | Upward movement, enemy collision |
| `EnemyController` | Enemy Prefab | Downward + sway movement |
| `EnemySpawner` | Empty GameObject | Spawns enemies with increasing difficulty |
| `GameManager` | Empty GameObject | Singleton: score, game state, restart |
| `UIManager` | Canvas | Singleton: score text, health text, Game Over panel |
| `BackgroundScroller` | Background Quad | Scrolls texture for parallax effect |
| `StarfieldGenerator` | Empty GameObject | Procedural particle starfield |

---

**Happy shooting! 🎮🚀**
