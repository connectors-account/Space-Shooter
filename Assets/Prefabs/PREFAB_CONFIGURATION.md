# Prefab Configuration (Runtime Generated)

This project creates all gameplay entities from code at runtime (no manual prefab drag/drop required).

## Player "virtual prefab"
Created in `EntityFactory.CreatePlayer` with:
- `SpriteRenderer` (triangle player ship)
- `CircleCollider2D` (`isTrigger = true`)
- `Rigidbody2D` (`isKinematic = true`, `gravityScale = 0`)
- `PlayerController`
- Child `Muzzle` transform
- Child `ShieldVisual` object + `SpriteRenderer`

## Enemy "virtual prefab"
Created in `EntityFactory.CreateEnemy`:
- `SpriteRenderer` (color varies by enemy type)
- `CircleCollider2D` trigger
- `Rigidbody2D` kinematic
- `EnemyController`

Enemy types and defaults:
- Basic: HP 20, score 100, single straight bullet
- ZigZag: HP 15, score 130, aimed bullets
- Tank: HP 45, score 220, 3-way spread bullets

## Bullet "virtual prefab"
Created in `EntityFactory.CreateBullet`:
- `SpriteRenderer`
- `BoxCollider2D` trigger
- `Rigidbody2D` kinematic
- `BulletController`

## Power-up "virtual prefab"
Created in `EntityFactory.CreatePowerUp`:
- `SpriteRenderer`
- `CircleCollider2D` trigger
- `Rigidbody2D` kinematic
- `PowerUpController`

Types:
- `Health`: +30 HP (clamped to max)
- `RapidFire`: faster fire rate for 6 seconds
- `Shield`: invulnerability for 8 seconds
