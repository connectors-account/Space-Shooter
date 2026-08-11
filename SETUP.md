# Space Shooter — Unity Desktop Game (Windows)

A simple, complete 2D space shooter for **Windows desktop**, built with **Unity + C#**.
This repository contains **all the C# source code (no placeholders)** plus this step-by-step
guide to assemble the scenes/prefabs in the Unity Editor and build a Windows `.exe`.

> ⚠️ The `.exe` cannot be pre-compiled here because the Unity Editor is not part of this
> delivery. Unity is **free** — installing it and following the steps below will get you a
> running `.exe` in a few minutes.

---

## 0. What you get

```
space_shooter_unity/
├── Assets/
│   ├── Scripts/
│   │   ├── GameManager.cs        # score, lives, game-over state
│   │   ├── PlayerController.cs   # move (WASD/arrows) + shoot (Space)
│   │   ├── Bullet.cs             # player & enemy projectiles
│   │   ├── EnemyController.cs    # enemy movement + shooting + death
│   │   ├── EnemySpawner.cs       # periodic enemy spawns from the top
│   │   ├── UIManager.cs          # HUD (score/lives) + Game Over panel
│   │   ├── MainMenu.cs           # Start / Quit buttons
│   │   ├── ParallaxBackground.cs # single-layer scrolling background
│   │   ├── AudioManager.cs       # shoot / explosion / game-over SFX
│   │   └── SelfDestruct.cs       # cleans up temporary effects
│   ├── Prefabs/   (you create these in the Editor — see Section 4)
│   ├── Scenes/    (you create MainMenu + Game — see Sections 3 & 5)
│   ├── Sprites/   (built-in Unity sprites or your own art)
│   ├── Audio/     (drop your .wav/.mp3 clips here)
│   └── UI/
└── SETUP.md  (this file)
```

**Controls:** Move = WASD or Arrow keys · Shoot = Space · Menus = Mouse.

---

## 1. Install Unity

1. Download **Unity Hub** from <https://unity.com/download>.
2. In Unity Hub → **Installs** → **Install Editor** → choose **Unity 2022.3 LTS**
   (any 2021.3+ or 6.x LTS works too).
3. During install, under **Platforms**, make sure **"Windows Build Support (IL2CPP)"**
   is checked (it is included by default on Windows).

---

## 2. Create the project and import the scripts

1. Unity Hub → **Projects** → **New project**.
2. Template: **2D (Core)** (or "2D"). Name it `SpaceShooter`, pick a location, **Create**.
3. When the Editor opens, close it, then **copy the `Assets/Scripts` folder from this
   delivery into your new project's `Assets/` folder** (merge — keep Unity's existing files).
   - Easiest: in your file explorer, drag the provided `Scripts` folder into the project's
     `Assets` folder. Also create empty `Prefabs`, `Scenes`, `Sprites`, `Audio`, `UI`
     folders inside `Assets` (right-click in the Project window → Create → Folder).
4. Back in Unity, wait for it to compile. Open **Window → General → Console** and confirm
   there are **no errors**. All scripts live in the `SpaceShooter` C# namespace.

---

## 3. Build the Game scene

Create the main gameplay scene.

1. **File → New Scene → Basic 2D** (or empty). **File → Save As** → `Assets/Scenes/Game.unity`.
2. Select the **Main Camera**:
   - Projection = **Orthographic**, **Size = 5**.
   - Set a dark background color (e.g. near-black) via the Camera's *Background* field.
   - Make sure its **Tag = MainCamera** (default).

### 3a. Simple sprites (no custom art needed)
You can use Unity's built-in sprites. In the Project window click **+ → 2D → Sprites →
Square** (and **Triangle**) to create sprite assets in `Assets/Sprites`. You'll tint them
with the SpriteRenderer *Color* field. (Alternatively use the built-in "Knob" sprite or your
own PNGs — just drop PNGs into `Assets/Sprites` and set their *Texture Type = Sprite (2D and UI)*.)

### 3b. Player
1. **GameObject → 2D Object → Sprite → Square**. Rename to **Player**.
   - SpriteRenderer: use a Triangle sprite if you prefer a "ship" look; set *Color* to cyan/green.
   - Scale ≈ (0.6, 0.6, 1). Position ≈ (0, -3, 0).
2. **Add Component → Box Collider 2D** (or Polygon Collider 2D). Tick **Is Trigger = ON**.
3. **Add Component → Rigidbody 2D**. Set **Body Type = Kinematic** (or Dynamic with Gravity Scale = 0).
4. **Tag** the Player: in the Inspector top-left *Tag* dropdown → **Add Tag…** → create
   tags **Player** and **Enemy**. Then set this object's Tag = **Player**.
5. **Add Component → Player Controller** (the script).
6. Create a child empty for the muzzle: right-click Player → **Create Empty** → rename
   **FirePoint**, position it just above the ship (local y ≈ +0.5). Drag **FirePoint** into
   the PlayerController's *Fire Point* slot.
7. Leave *Bullet Prefab* empty for now — you'll assign it after Section 4.

### 3c. Managers
1. **GameObject → Create Empty** → rename **GameManager**. Add Component → **Game Manager**.
2. **GameObject → Create Empty** → rename **AudioManager**. Add Component → **Audio Manager**
   (it auto-adds an AudioSource). Assign your `shoot`, `explosion`, `gameOver` clips (Section 7).
3. **GameObject → Create Empty** → rename **EnemySpawner**. Add Component → **Enemy Spawner**.
   (Assign *Enemy Prefab* after Section 4.)

### 3d. Scrolling background (single layer)
- **Simplest (transform loop):** create a large Square sprite named **Background**, tint it dark,
  scale it to cover the screen (e.g. 12×12), position z = +5 (behind everything).
  Duplicate it and stack the copy directly above (y offset = its height) and parent both under an
  empty **BackgroundRoot**. Add **ParallaxBackground** to each background sprite, set
  *Use Material Scroll = OFF*, tune *scrollSpeed*, and set the reset thresholds so it loops.
- **Or (UV scroll):** if you have a seamless tiling star texture, put it on a Quad with a material,
  add **ParallaxBackground** with *Use Material Scroll = ON*.

---

## 4. Create the Prefabs

Prefabs are created inside the Editor (binary Unity assets can't be hand-written), so build
them once here.

### 4a. Player Bullet prefab
1. **GameObject → 2D Object → Sprite → Square**, rename **PlayerBullet**, tint yellow,
   scale ≈ (0.15, 0.4, 1).
2. Add **Box Collider 2D** → *Is Trigger = ON*.
3. Add **Rigidbody 2D** → *Body Type = Kinematic*, *Gravity Scale = 0*.
4. Add **Bullet** script.
5. Drag **PlayerBullet** from the Hierarchy into `Assets/Prefabs` to make it a prefab, then
   delete it from the scene.
6. Select the **Player** in the scene → drag the **PlayerBullet** prefab into its
   PlayerController *Bullet Prefab* slot.

### 4b. Enemy Bullet prefab
1. Same as 4a but rename **EnemyBullet**, tint red/orange. Add Bullet script, collider (trigger),
   Rigidbody2D (kinematic). Save as prefab in `Assets/Prefabs`, delete from scene.

### 4c. Enemy prefab
1. **GameObject → 2D Object → Sprite → Square** (or Triangle), rename **Enemy**, tint magenta/red,
   scale ≈ (0.6, 0.6, 1).
2. Add **Box Collider 2D** → *Is Trigger = ON*.
3. Set its **Tag = Enemy**.
4. Add **Enemy Controller** script. In its Inspector:
   - *Enemy Bullet Prefab* → drag the **EnemyBullet** prefab.
   - Adjust *Move Speed*, *Fire Interval*, *Score Value* to taste. Uncheck *Can Shoot* for a
     pure "dodge" enemy if you like.
5. Save **Enemy** as a prefab in `Assets/Prefabs`, delete from scene.
6. Select **EnemySpawner** → drag the **Enemy** prefab into its *Enemy Prefab* slot.

### 4d. (Optional) Explosion prefab
1. Create a small Square/particle, tint white/orange, add **SelfDestruct** (lifetime ≈ 0.4s).
   Save as prefab. Drag it into the Enemy prefab's *Explosion Prefab* slot if you want a pop effect.

---

## 5. Build the HUD & Game Over UI (in the Game scene)

1. **GameObject → UI → Canvas** (this also creates an EventSystem — keep it).
   - Canvas *Render Mode* = **Screen Space - Overlay**.
   - On the Canvas Scaler set *UI Scale Mode* = **Scale With Screen Size**, reference 1920×1080.
2. **Score text:** right-click Canvas → **UI → Legacy → Text**. Rename **ScoreText**, anchor
   top-left, set text "Score: 0", font size ~28, color white.
3. **Lives text:** another **UI → Legacy → Text**, rename **LivesText**, anchor top-right,
   text "Lives: 3".
4. **Game Over panel:**
   - Right-click Canvas → **UI → Panel**, rename **GameOverPanel**. Give it a semi-transparent
     dark color.
   - Inside it add a **Legacy → Text** named **FinalScoreText** ("Final Score: 0"), centered, large.
   - Add **UI → Legacy → Button**, rename **RestartButton**, set its child Text to "Restart".
   - Add another **Button**, **MenuButton**, text "Main Menu".
5. **Wire the UIManager:**
   - Add **UIManager** component to the **Canvas** (or a new empty **UIManager** object).
   - Drag **ScoreText** → *Score Text*, **LivesText** → *Lives Text*,
     **GameOverPanel** → *Game Over Panel*, **FinalScoreText** → *Final Score Text*.
   - Confirm *Game Scene Name* = `Game` and *Main Menu Scene Name* = `MainMenu`.
6. **Hook button clicks:**
   - Select **RestartButton** → in *Button → On Click ()* click **+**, drag the UIManager object
     into the slot, choose **UIManager → RestartGame()**.
   - Select **MenuButton** → On Click → UIManager → **GoToMainMenu()**.
7. Select **GameManager** in the Hierarchy → drag the **UIManager** object into its
   *Ui Manager* slot (or leave empty — it auto-finds one at runtime).
8. **Deactivate GameOverPanel** for normal play: with GameOverPanel selected, uncheck the box
   next to its name at the top of the Inspector (the script also hides it on start).

**Save the scene** (Ctrl+S).

---

## 6. Build the Main Menu scene

1. **File → New Scene → Basic 2D** → **Save As** `Assets/Scenes/MainMenu.unity`.
2. Add a **Canvas** (+ EventSystem).
3. Add a **Legacy → Text** title "SPACE SHOOTER", centered near the top.
4. Add a **Button** "Start" and a **Button** "Quit".
5. **GameObject → Create Empty** → **MainMenu** object → add **MainMenu** script.
   Confirm *Game Scene Name* = `Game`.
6. **Start** button → On Click → MainMenu object → **MainMenu.StartGame()**.
   **Quit** button → On Click → MainMenu object → **MainMenu.QuitGame()**.
7. (Optional) add a background sprite. **Save the scene.**

---

## 7. Add sound effects (optional but supported)

1. Drop your own `.wav`/`.mp3` files into `Assets/Audio` (e.g. `shoot.wav`, `explosion.wav`,
   `gameover.wav`). Free sources: <https://freesound.org> or <https://sfxr.me> (make your own).
2. Select the **AudioManager** object → assign the three clips to *Shoot Clip*, *Explosion Clip*,
   *Game Over Clip*. Adjust the volume sliders if needed. Done — the scripts already call them.

---

## 8. Register scenes for the build

1. **File → Build Settings…**
2. Open **MainMenu.unity** first, click **Add Open Scenes**. Then open **Game.unity** and
   **Add Open Scenes**.
3. In the *Scenes In Build* list, make sure **MainMenu is index 0** (drag it to the top so the
   game launches at the menu). **Game** should be index 1.

---

## 9. Build the Windows `.exe`

1. Still in **File → Build Settings…**, select **Platform = Windows, Mac, Linux** →
   *Target Platform* = **Windows**, *Architecture* = **x86_64**.
   - If it says "Switch Platform", click it and wait.
2. Click **Build** (or **Build And Run**).
3. Choose an output folder (e.g. `Builds/Windows`). Unity produces:
   - `SpaceShooter.exe`  ← double-click to play
   - a `SpaceShooter_Data/` folder and `UnityPlayer.dll` (keep these next to the .exe).
4. Zip the whole build folder to share it.

---

## 10. Quick test checklist

- [ ] Play the **MainMenu** scene → **Start** loads the Game scene.
- [ ] Ship moves with WASD/arrows and stays on screen.
- [ ] **Space** fires yellow bullets upward; hitting an enemy destroys it and adds score.
- [ ] Enemies spawn from the top periodically and shoot downward.
- [ ] Getting hit reduces **Lives**; at 0 the **Game Over** panel shows the final score.
- [ ] **Restart** reloads the Game scene; **Main Menu** returns to the menu.

---

## Notes & customization

- **Difficulty:** tune `EnemySpawner.spawnInterval`, `EnemyController.moveSpeed`,
  `fireInterval`, and `PlayerController.fireCooldown` in the Inspector.
- **Lives:** change `GameManager.startingLives`.
- **All gameplay values are exposed in the Inspector** — no code editing required for tuning.
- Everything is in the `SpaceShooter` namespace and uses only built-in Unity modules
  (`UnityEngine`, `UnityEngine.UI`, `UnityEngine.SceneManagement`) — no extra packages.
