# Prefab Configuration Reference

This document describes the configuration for each prefab in detail.
If you use the automated setup (`Tools > Space Shooter > Setup Entire Project`), all prefabs are created automatically.

---

## Player Prefab

**GameObject: Player**
- **Tag**: `Player`
- **Components**:
  - `SpriteRenderer`: Sprite = PlayerShip.png, Sorting Order = 10
  - `BoxCollider2D`: Is Trigger = true, Size = (0.5, 0.7)
  - `Rigidbody2D`: Body Type = Kinematic
  - `PlayerController`:
    - Move Speed: 8
    - Boundary X: 8.5
    - Boundary Y: 4.5
    - Bullet Prefab: → PlayerBullet prefab
    - Fire Point: → child FirePoint transform
    - Fire Rate: 0.25
    - Max Health: 100
    - Shield Visual: → child ShieldVisual object
    - Rapid Fire Rate: 0.1
    - Power Up Duration: 5

**Children**:
- `FirePoint` (empty object at local position (0, 0.5, 0))
- `ShieldVisual` (Sprite = ShieldBubble.png, Sorting Order = 11, starts disabled)

---

## Enemy Prefabs

### EnemyStraight
- **Tag**: `Enemy`
- **Sprite**: EnemyStraight.png, Sorting Order = 3
- **BoxCollider2D**: Is Trigger = true, Size = (0.6, 0.6)
- **Rigidbody2D**: Kinematic
- **EnemyController**:
  - Enemy Type: Straight
  - Bullet Prefab: → EnemyBullet prefab
  - Power Up Prefabs: [PowerUpHealth, PowerUpRapidFire, PowerUpShield]
  - Drop Chance: 0.15

### EnemyZigzag
- Same as Straight but:
  - Sprite: EnemyZigzag.png
  - Enemy Type: Zigzag
  - Zigzag Amplitude: 3, Frequency: 2

### EnemySwooper
- Same structure but:
  - Sprite: EnemySwooper.png
  - Enemy Type: Swooper
  - Swoop Radius: 4

### EnemyTank
- Same structure but:
  - Sprite: EnemyTank.png
  - Enemy Type: Tank
  - Size: slightly larger collider (0.7, 0.7)

---

## Bullet Prefabs

### PlayerBullet
- **Tag**: `PlayerBullet`
- **Sprite**: PlayerBullet.png, Sorting Order = 5
- **BoxCollider2D**: Is Trigger = true, Size = (0.2, 0.4)
- **BulletController**:
  - Speed: 14
  - Damage: 10
  - Direction: (0, 1) (up)
  - Is Player Bullet: true

### EnemyBullet
- **Tag**: `EnemyBullet`
- **Sprite**: EnemyBullet.png, Sorting Order = 5
- **BoxCollider2D**: Is Trigger = true, Size = (0.2, 0.4)
- **BulletController**:
  - Speed: 8
  - Damage: 10
  - Direction: (0, -1) (down)
  - Is Player Bullet: false

---

## Power-Up Prefabs

### PowerUpHealth
- **Tag**: `PowerUp`
- **Sprite**: PowerUpHealth.png, Sorting Order = 4
- **CircleCollider2D**: Is Trigger = true, Radius = 0.3
- **PowerUpController**: Type = Health

### PowerUpRapidFire
- Same but Type = RapidFire, Sprite = PowerUpRapidFire.png

### PowerUpShield
- Same but Type = Shield, Sprite = PowerUpShield.png
