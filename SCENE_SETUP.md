# Scene & Prefab Setup Guide

## Quick-Start (Automatic — Recommended)

The game includes a **SceneBootstrap** script that builds the entire scene at runtime,
including procedural sprites. No manual setup or imported art needed.

### Steps:
1. Open Unity and create a new 2D project (or import this folder as a project).
2. In the **Hierarchy**, create an empty GameObject → name it `Bootstrap`.
3. Drag `Assets/Scripts/SceneBootstrap.cs` onto it.
4. Press **Play** — the full game launches (main menu → play → waves → game over).

That's it! Everything is generated at runtime.

---

## Manual Setup (for customisation)

If you prefer to set things up manually (to use your own art, tweak prefabs, etc.):

### 1. Required Tags
Go to **Edit → Project Settings → Tags and Layers** and add:
- `Player`
- `Enemy`
- `PlayerBullet`
- `EnemyBullet`
- `PowerUp`

> The included `TagSetup.cs` editor script does this automatically on compilation.

### 2. Create Prefabs

#### Player Prefab
1. Create a new GameObject with a **SpriteRenderer** (assign your ship sprite).
2. Add **Rigidbody2D** (Gravity Scale = 0, Freeze Rotation = checked).
3. Add **BoxCollider2D** (Is Trigger = checked, size ~0.6 × 0.8).
4. Add the **PlayerController** script.
5. Create a child empty GameObject named `FirePoint` at local position (0, 0.6, 0).
6. Assign `FirePoint` to the `firePoint` field on PlayerController.
7. Tag as `Player`.
8. Drag to `Assets/Prefabs/`.

#### Bullet Prefab
1. Create a small sprite (4×4 px circle or square).
2. Add **Rigidbody2D** (Kinematic).
3. Add **BoxCollider2D** (Is Trigger, size ~0.2 × 0.2).
4. Add the **Bullet** script.
5. Drag to `Assets/Prefabs/`.
6. Assign this prefab to PlayerController's `bulletPrefab` and Enemy's `bulletPrefab`.

#### Enemy Prefabs
Create variants (Basic, Zigzag, Sine, Diver):
1. Sprite + **Rigidbody2D** (Kinematic) + **BoxCollider2D** (Is Trigger).
2. Add the **Enemy** script. Set `pattern`, `health`, `scoreValue`.
3. Assign `bulletPrefab` and `powerUpPrefab` references.
4. Tag as `Enemy`.
5. Drag to `Assets/Prefabs/`.

#### Power-Up Prefab
1. Small sprite + **BoxCollider2D** (Is Trigger).
2. Add the **PowerUp** script. Set `randomizeOnSpawn = true`.
3. Tag as `PowerUp`.
4. Drag to `Assets/Prefabs/`.

### 3. Scene Hierarchy (Manual)

```
Main Camera  (Orthographic, Size 5)
├── GameManager        [GameManager.cs]
├── SoundManager       [SoundManager.cs]
├── Player             [PlayerController.cs]  (or instantiate from prefab)
│   └── FirePoint
├── EnemySpawner       [EnemySpawner.cs]
│       → enemyPrefabs: [Enemy_Basic, Enemy_Zigzag, Enemy_Sine, Enemy_Diver]
├── BG_Layer1          [BackgroundScroller.cs, scrollSpeed=0.5]
├── BG_Layer2          [BackgroundScroller.cs, scrollSpeed=1.2]
├── UICanvas           [UIManager.cs]
└── EventSystem        [EventSystem + StandaloneInputModule]
```

### 4. Background Sprites
- Create two starfield images (256×512 px, dark with white dots).
- Set **Texture Type = Sprite**, **Wrap Mode = Repeat** (if using material offset).
- Assign to BG_Layer1 and BG_Layer2 SpriteRenderers.
- Set Sorting Order to -10 and -9 respectively.

### 5. Sound Setup
On the SoundManager, add entries to the `sfxEntries` array with these names:
- `PlayerShoot`
- `EnemyShoot`
- `Explosion`
- `PlayerHit`
- `PowerUp`

Assign any short .wav/.ogg clips you like. Free SFX sources:
- [Freesound.org](https://freesound.org)
- [OpenGameArt.org](https://opengameart.org)
- [Kenney.nl](https://kenney.nl/assets)
