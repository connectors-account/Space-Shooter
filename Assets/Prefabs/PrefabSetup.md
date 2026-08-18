# Prefab Setup

Create these prefabs under `Assets/Prefabs/`. For each: build the GameObject in a
scene, add the listed components, then drag it into the Prefabs folder.

Recommended **Tags**: `Player`, `Enemy`, `EnemyBullet`, `PlayerBullet`, `PowerUp`.
Recommended **Layers**: `Player`, `Enemy`, `PlayerBullet`, `EnemyBullet`, `PowerUp`.
Configure the **Physics 2D** collision matrix (Edit → Project Settings → Physics 2D)
so that PlayerBullet↔Enemy and EnemyBullet↔Player collide, and player/enemy bullets
ignore their own side.

All colliders are **2D triggers** (`Is Trigger = true`) and every moving object that
uses triggers needs a **Rigidbody2D** (Gravity Scale 0). One Rigidbody2D per pair is
enough for trigger events; the bullets/enemies carry theirs.

---

## Player  (tag: Player, layer: Player)
| Component | Settings |
|-----------|----------|
| SpriteRenderer | Sprite = `player` (blue triangle), Order in Layer = 5 |
| Rigidbody2D | Body Type = Kinematic, Gravity Scale = 0 |
| BoxCollider2D / PolygonCollider2D | Is Trigger = true, sized to the triangle |
| `PlayerController` | moveSpeed = 8, edgePadding = 0.5 |
| `PlayerHealth` | maxHealth = 3, invincibilityDuration = 1.5 |
| `PlayerShooter` | fireMode = Single, fireRate = 0.2, firePoint = child "Muzzle" |
| child `Muzzle` (empty) | positioned at the ship nose (local 0, 0.5, 0) |

## PlayerBullet  (tag: PlayerBullet, layer: PlayerBullet)
| Component | Settings |
|-----------|----------|
| SpriteRenderer | Sprite = `bullet_player` (cyan rectangle), Order = 4 |
| Rigidbody2D | Kinematic, Gravity Scale = 0 |
| BoxCollider2D | Is Trigger = true, ~0.1 × 0.4 |
| `PlayerBullet` | speed = 15, damage = 1, lifeTime = 5 (set in Awake) |

## EnemyBullet  (tag: EnemyBullet, layer: EnemyBullet)
| Component | Settings |
|-----------|----------|
| SpriteRenderer | Sprite = `bullet_enemy` (magenta rectangle), Order = 4 |
| Rigidbody2D | Kinematic, Gravity Scale = 0 |
| BoxCollider2D | Is Trigger = true, ~0.1 × 0.4 |
| `EnemyBullet` | speed = 6, damage = 1, lifeTime = 5 (set in Awake) |

## EnemyDrone  (tag: Enemy, layer: Enemy)
| Component | Settings |
|-----------|----------|
| SpriteRenderer | Sprite = `enemy_drone` (red circle), Order = 5 |
| Rigidbody2D | Kinematic, Gravity Scale = 0 |
| CircleCollider2D | Is Trigger = true, radius ≈ 0.4 |
| `EnemyHealth` | (configured to HP 1 / score 100 by EnemyDrone) |
| `EnemyMovement` | pattern = StraightDown or Sine, speed = 3 |
| `EnemyDrone` | shootInterval = 2, powerUpDrops = [Speed, Shield, TripleShot, Bomb] |

## EnemyFighter  (tag: Enemy, layer: Enemy)
| Component | Settings |
|-----------|----------|
| SpriteRenderer | Sprite = `enemy_fighter` (orange diamond), Order = 5 |
| Rigidbody2D | Kinematic, Gravity Scale = 0 |
| PolygonCollider2D | Is Trigger = true (diamond shape) |
| `EnemyHealth` | (HP 3 / score 250 via EnemyFighter) |
| `EnemyMovement` | pattern = Zigzag, speed = 3, amplitude = 2 |
| `EnemyFighter` | shootInterval = 1.5, powerUpDrops = [...] |

## EnemyBoss  (tag: Enemy, layer: Enemy)
| Component | Settings |
|-----------|----------|
| SpriteRenderer | Sprite = `enemy_boss` (large purple hexagon), Order = 6, scale ≈ 3 |
| Rigidbody2D | Kinematic, Gravity Scale = 0 |
| PolygonCollider2D | Is Trigger = true (hexagon) |
| `EnemyHealth` | (HP 50 / score 5000 via EnemyBoss) |
| `EnemyBoss` | sweepSpeed = 2, sweepHalfWidth = 5, chargeSpeed = 6, bulletSpread = 30 |
| child `ShieldFX` (optional) | SpriteRenderer, enabled during the 50% shield phase |

> Note: the boss provides its own movement (horizontal sweep + phase-3 charge), so
> it does **not** require an `EnemyMovement` component.

## PowerUpSpeed  (tag: PowerUp, layer: PowerUp)
| Component | Settings |
|-----------|----------|
| SpriteRenderer | Sprite = `powerup_speed` (yellow circle + » icon) |
| Rigidbody2D | Kinematic, Gravity Scale = 0 |
| CircleCollider2D | Is Trigger = true, radius ≈ 0.4 |
| `PowerUpSpeed` | duration = 8, speedBonus = 4, fallSpeed = 2 |

## PowerUpShield  (tag: PowerUp)
| SpriteRenderer | Sprite = `powerup_shield` (cyan circle + ⛨ icon) |
| Rigidbody2D + CircleCollider2D (trigger) | Gravity 0 |
| `PowerUpShield` | shieldAmount = 3, duration = 0 |

## PowerUpTripleShot  (tag: PowerUp)
| SpriteRenderer | Sprite = `powerup_triple` (green circle + ⋔ icon) |
| Rigidbody2D + CircleCollider2D (trigger) | Gravity 0 |
| `PowerUpTripleShot` | duration = 10 |

## PowerUpBomb  (tag: PowerUp)
| SpriteRenderer | Sprite = `powerup_bomb` (red circle + ✸ icon) |
| Rigidbody2D + CircleCollider2D (trigger) | Gravity 0 |
| `PowerUpBomb` | pointsPerEnemy = 50, duration = 0 |

## Background layer (one per parallax layer)
| Component | Settings |
|-----------|----------|
| `ParallaxBackground` | scrollSpeed = 0.5 / 1.0 / 2.0, tileHeight = 10 |
| child `TileA`, child `TileB` | SpriteRenderer = `stars` tiling sprite; stacked vertically 10 units apart; Order = -10 |

---

### Wiring the pools & spawner
* **BulletPool** (scene object): assign `playerBulletPrefab` = PlayerBullet prefab,
  `enemyBulletPrefab` = EnemyBullet prefab, `initialPoolSize` = 30.
* **EnemySpawner** (scene object): assign `dronePrefab`, `fighterPrefab`,
  `bossPrefab`.
* **WaveManager** (scene object): assign `spawner` = the EnemySpawner.
