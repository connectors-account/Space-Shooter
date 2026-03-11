# Prefab Definitions

This document describes all prefabs needed for the Space Shooter game.

## Player Prefabs

### Player Ship
**File:** `Assets/Prefabs/Player.prefab`

```
GameObject: Player
├── Tag: "Player"
├── Layer: "Player"
├── Components:
│   ├── Transform
│   │   └── Position: (0, -3, 0)
│   ├── SpriteRenderer
│   │   ├── Sprite: player_ship.png
│   │   ├── Color: White
│   │   └── Sorting Layer: "Characters" (Order: 10)
│   ├── Rigidbody2D
│   │   ├── Body Type: Dynamic
│   │   ├── Gravity Scale: 0
│   │   └── Freeze Rotation: Z = true
│   ├── BoxCollider2D or PolygonCollider2D
│   │   └── Is Trigger: true
│   ├── PlayerController (Script)
│   │   ├── Move Speed: 8
│   │   ├── Fire Rate: 0.2
│   │   ├── Bullet Prefab: PlayerBullet
│   │   └── Fire Point: FirePoint child object
│   ├── HealthSystem (Script)
│   │   └── Max Health: 100
│   └── AudioSource
│       └── Play On Awake: false
└── Children:
    └── FirePoint
        └── Position: (0, 0.5, 0)
```

### Player Bullet
**File:** `Assets/Prefabs/PlayerBullet.prefab`

```
GameObject: PlayerBullet
├── Tag: "PlayerBullet"
├── Layer: "PlayerBullets"
├── Components:
│   ├── Transform
│   ├── SpriteRenderer
│   │   ├── Sprite: bullet_player.png
│   │   ├── Color: Cyan (0, 1, 1, 1)
│   │   └── Sorting Layer: "Projectiles" (Order: 5)
│   ├── Rigidbody2D
│   │   ├── Body Type: Dynamic
│   │   ├── Gravity Scale: 0
│   │   └── Collision Detection: Continuous
│   ├── CapsuleCollider2D or CircleCollider2D
│   │   └── Is Trigger: true
│   └── Bullet (Script)
│       ├── Speed: 12
│       ├── Damage: 1
│       ├── Is Player Bullet: true
│       └── Lifetime: 5
```

---

## Enemy Prefabs

### Basic Enemy
**File:** `Assets/Prefabs/BasicEnemy.prefab`

```
GameObject: BasicEnemy
├── Tag: "Enemy"
├── Layer: "Enemies"
├── Components:
│   ├── SpriteRenderer
│   │   ├── Sprite: enemy_basic.png
│   │   ├── Color: Red (1, 0.3, 0.3, 1)
│   │   └── Sorting Layer: "Characters" (Order: 5)
│   ├── Rigidbody2D
│   │   ├── Body Type: Kinematic
│   │   └── Gravity Scale: 0
│   ├── BoxCollider2D or CircleCollider2D
│   │   └── Is Trigger: true
│   ├── EnemyBase (Script)
│   │   ├── Enemy Type: Basic
│   │   ├── Move Speed: 3
│   │   ├── Score Value: 100
│   │   ├── Can Shoot: true
│   │   ├── Fire Rate: 2
│   │   └── Bullet Prefab: EnemyBullet
│   └── HealthSystem (Script)
│       └── Max Health: 20
```

### Zigzag Enemy
**File:** `Assets/Prefabs/ZigzagEnemy.prefab`

```
GameObject: ZigzagEnemy
├── Tag: "Enemy"
├── Components:
│   ├── SpriteRenderer
│   │   └── Color: Orange (1, 0.6, 0.2, 1)
│   ├── EnemyBase (Script)
│   │   ├── Enemy Type: Zigzag
│   │   ├── Move Speed: 4
│   │   ├── Score Value: 150
│   │   ├── Zigzag Amplitude: 2
│   │   └── Zigzag Frequency: 2
│   └── HealthSystem
│       └── Max Health: 15
```

### Circular Enemy
**File:** `Assets/Prefabs/CircularEnemy.prefab`

```
GameObject: CircularEnemy
├── Tag: "Enemy"
├── Components:
│   ├── SpriteRenderer
│   │   └── Color: Purple (0.8, 0.3, 1, 1)
│   ├── EnemyBase (Script)
│   │   ├── Enemy Type: Circular
│   │   ├── Move Speed: 2
│   │   ├── Score Value: 200
│   │   ├── Circular Radius: 1.5
│   │   └── Circular Speed: 3
│   └── HealthSystem
│       └── Max Health: 25
```

### Charger Enemy
**File:** `Assets/Prefabs/ChargerEnemy.prefab`

```
GameObject: ChargerEnemy
├── Tag: "Enemy"
├── Components:
│   ├── SpriteRenderer
│   │   └── Color: Yellow (1, 1, 0.3, 1)
│   ├── EnemyBase (Script)
│   │   ├── Enemy Type: Charger
│   │   ├── Move Speed: 5
│   │   ├── Score Value: 250
│   │   └── Can Shoot: false
│   └── HealthSystem
│       └── Max Health: 10
```

### Boss Enemy
**File:** `Assets/Prefabs/BossEnemy.prefab`

```
GameObject: BossEnemy
├── Tag: "Enemy"
├── Components:
│   ├── SpriteRenderer
│   │   ├── Sprite: enemy_boss.png (larger sprite)
│   │   └── Color: Dark Red (0.8, 0, 0, 1)
│   ├── BossEnemy (Script)
│   │   ├── Move Speed: 1.5
│   │   ├── Score Value: 1000
│   │   ├── Fire Rate: 1
│   │   ├── Spread Shot Count: 5
│   │   └── Circular Shot Count: 12
│   └── HealthSystem
│       └── Max Health: 500
```

### Enemy Bullet
**File:** `Assets/Prefabs/EnemyBullet.prefab`

```
GameObject: EnemyBullet
├── Tag: "EnemyBullet"
├── Layer: "EnemyBullets"
├── Components:
│   ├── SpriteRenderer
│   │   ├── Color: Red (1, 0.3, 0.3, 1)
│   │   └── Sorting Layer: "Projectiles"
│   ├── Rigidbody2D
│   │   └── Gravity Scale: 0
│   ├── CircleCollider2D
│   │   └── Is Trigger: true
│   └── Bullet (Script)
│       ├── Speed: 6
│       ├── Damage: 1
│       ├── Is Player Bullet: false
│       └── Lifetime: 5
```

---

## Power-Up Prefabs

### PowerUp
**File:** `Assets/Prefabs/PowerUp.prefab`

```
GameObject: PowerUp
├── Tag: "PowerUp"
├── Layer: "PowerUps"
├── Components:
│   ├── SpriteRenderer
│   │   ├── Sprite: powerup.png (diamond or star shape)
│   │   └── Sorting Layer: "Pickups" (Order: 8)
│   ├── CircleCollider2D
│   │   ├── Is Trigger: true
│   │   └── Radius: 0.3
│   └── PowerUp (Script)
│       ├── Move Speed: 2
│       ├── Lifetime: 10
│       ├── Duration: 5 (for timed power-ups)
│       └── Amount: 25 (for health/score)
```

---

## Manager Prefabs

### GameManager
**File:** `Assets/Prefabs/GameManager.prefab`

```
GameObject: GameManager
├── Components:
│   ├── GameManager (Script)
│   │   ├── Starting Lives: 3
│   │   └── Invincibility Duration: 2
│   ├── ScoreManager (Script)
│   └── AudioManager (Script)
│       ├── Music Source: (child AudioSource)
│       └── SFX Source: (child AudioSource)
└── Children:
    ├── MusicSource (AudioSource component)
    └── SFXSource (AudioSource component)
```

### ObjectPooler
**File:** `Assets/Prefabs/ObjectPooler.prefab`

```
GameObject: ObjectPooler
├── Components:
│   └── ObjectPooler (Script)
│       └── Pools:
│           ├── Tag: "PlayerBullet", Size: 30
│           ├── Tag: "EnemyBullet", Size: 50
│           ├── Tag: "BasicEnemy", Size: 20
│           ├── Tag: "ZigzagEnemy", Size: 15
│           ├── Tag: "CircularEnemy", Size: 15
│           ├── Tag: "ChargerEnemy", Size: 10
│           └── Tag: "PowerUp", Size: 10
```

---

## Background Prefabs

### ParallaxBackground
**File:** `Assets/Prefabs/ParallaxBackground.prefab`

```
GameObject: ParallaxBackground
├── Components:
│   ├── ParallaxBackground (Script)
│   │   └── Scroll Speed: 0.5
│   └── StarfieldGenerator (Script)
│       ├── Star Count: 100
│       ├── Field Width: 20
│       └── Field Height: 15
└── Children:
    ├── BackgroundLayer1 (furthest, slowest)
    │   └── SpriteRenderer (space background)
    └── BackgroundLayer2 (closer, faster)
        └── SpriteRenderer (nebula/stars overlay)
```

---

## UI Prefabs

### Canvas
The Canvas should be set up in each scene with the UIManager component:

```
Canvas
├── Canvas Scaler
│   ├── UI Scale Mode: Scale With Screen Size
│   ├── Reference Resolution: 1920 x 1080
│   └── Match: 0.5
├── UIManager (Script)
└── Children:
    ├── HUDPanel
    │   ├── ScoreText (TextMeshPro)
    │   ├── HighScoreText (TextMeshPro)
    │   ├── WaveText (TextMeshPro)
    │   ├── LivesText (TextMeshPro)
    │   ├── HealthBar (Slider)
    │   ├── MultiplierText (TextMeshPro)
    │   └── ComboText (TextMeshPro)
    ├── PausePanel
    │   ├── Title: "PAUSED"
    │   ├── ResumeButton
    │   ├── RestartButton
    │   └── MainMenuButton
    ├── GameOverPanel
    │   ├── Title: "GAME OVER"
    │   ├── FinalScoreText
    │   ├── FinalWaveText
    │   ├── HighScoreText
    │   ├── RestartButton
    │   └── MainMenuButton
    └── VictoryPanel
        ├── Title: "VICTORY!"
        ├── FinalScoreText
        ├── RestartButton
        └── MainMenuButton
```
