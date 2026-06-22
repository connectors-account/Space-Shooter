# MainGame Scene

Unity generates the actual binary/YAML `MainGame.unity` scene file when you save
the scene inside the Unity Editor. It cannot be meaningfully hand-written here,
so this placeholder documents exactly how to build the scene.

Follow **"Step 3: Build the Scene"** in the project root `README.md` to create
and wire up `MainGame.unity` in this folder.

Quick summary of GameObjects the scene needs:

| GameObject        | Components                                  |
|-------------------|---------------------------------------------|
| Main Camera       | Orthographic, size ~5, dark background      |
| GameManager       | GameManager, ScoreManager                   |
| BulletPool_Player | BulletPool (player bullet prefab)           |
| BulletPool_Enemy  | BulletPool (enemy bullet prefab)            |
| EnemySpawner      | EnemySpawner                                |
| Player            | PlayerController, HealthSystem, Collider2D, Rigidbody2D, SpriteRenderer, tag = "Player" |
| Background        | ParallaxBackground (+ 2 child sprite tiles) |
| Canvas            | UIManager (+ menu / HUD / game-over panels) |

Save it as `Assets/Scenes/MainGame.unity`.
