# Prefab Setup

Create these prefabs in Unity and assign references as listed.

## 1) Bullets

### PlayerBullet
- Tag: `PlayerBullet`
- Components:
  - SpriteRenderer (`player_bullet.png`)
  - BoxCollider2D (`Is Trigger = true`)
  - Rigidbody2D (`Body Type = Kinematic`)
  - `SpaceShooter.Weapons.BulletController`
- Script defaults:
  - Speed: `12`
  - Damage: `10`
  - Direction: `(0,1)`

### EnemyBullet
- Tag: `EnemyBullet`
- Components:
  - SpriteRenderer (`enemy_bullet.png`)
  - BoxCollider2D (`Is Trigger = true`)
  - Rigidbody2D (`Kinematic`)
  - `SpaceShooter.Weapons.BulletController`
- Script defaults:
  - Speed: `6`
  - Damage: `10`
  - Direction: `(0,-1)`

## 2) Player

### Player
- Tag: `Player`
- Components:
  - SpriteRenderer (`player_ship.png`)
  - BoxCollider2D (`Is Trigger = true`)
  - Rigidbody2D (`Kinematic`)
  - `SpaceShooter.Player.PlayerController`
- Children:
  - `FirePoint` at approx `(0, 0.45, 0)`
  - `ShieldVisual` with `shield_bubble.png` (inactive initially)
- Assign in PlayerController:
  - Bullet Prefab -> `PlayerBullet`
  - Fire Point -> `FirePoint`
  - Shield Visual -> `ShieldVisual`

## 3) Enemies

### EnemyBasic
- Tag: `Enemy`
- Sprite: `enemy_basic.png`
- `SpaceShooter.Enemy.EnemyController`
- Enemy Type: `Basic`
- Health 30, Speed 3, Fire Rate 1.5, Score 100

### EnemyZigzag
- Tag: `Enemy`
- Sprite: `enemy_zigzag.png`
- Enemy Type: `Zigzag`
- Health 20, Speed 4, Fire Rate 1.0, Score 150

### EnemyTank
- Tag: `Enemy`
- Sprite: `enemy_tank.png`
- Enemy Type: `Tank`
- Health 80, Speed 1.5, Fire Rate 2.5, Score 300

For each enemy prefab:
- Add child `FirePoint` around `(0,-0.35,0)`
- Assign Bullet Prefab -> `EnemyBullet`
- Assign Fire Point -> child `FirePoint`
- Assign Power Up Prefabs array -> all 3 power-up prefabs

## 4) Power-Ups

### PowerUpHealth
- Tag: `PowerUp`
- Sprite: `powerup_health.png`
- `SpaceShooter.PowerUps.PowerUpController`
- Type: `HealthPack`
- Heal Amount: `30`

### PowerUpRapidFire
- Sprite: `powerup_rapidfire.png`
- Type: `RapidFire`

### PowerUpShield
- Sprite: `powerup_shield.png`
- Type: `Shield`

Common components for all power-ups:
- CircleCollider2D (`Is Trigger = true`)
- Rigidbody2D (`Kinematic`)

## 5) SpawnManager Wiring

On `SpawnManager` object, assign:
- Basic Enemy Prefab -> `EnemyBasic`
- Zigzag Enemy Prefab -> `EnemyZigzag`
- Tank Enemy Prefab -> `EnemyTank`
