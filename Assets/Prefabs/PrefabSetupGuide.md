# Prefab Configuration Guide

## 1) Player Prefab (`Player.prefab`)

Components:
- `SpriteRenderer`
- `Collider2D` (BoxCollider2D recommended)
- `Rigidbody2D` (optional; if used, set Gravity Scale = 0)
- `PlayerController`

Setup:
- Tag: `Player`
- Create child object `FirePoint` at ship nose (e.g., y = +0.6)
- Assign:
  - `Player Bullet Prefab`
  - `Fire Point`

## 2) Enemy Prefabs

### A) Chaser Enemy (`Enemy_Chaser.prefab`)
Components:
- `SpriteRenderer`
- `Collider2D`
- `EnemyController`

EnemyController settings:
- Type: `Chaser`
- BaseHealth: `1`
- MoveSpeed: `2.8`
- ScoreValue: `100`

### B) ZigZag Enemy (`Enemy_ZigZag.prefab`)
Components:
- `SpriteRenderer`
- `Collider2D`
- `EnemyController`

EnemyController settings:
- Type: `ZigZag`
- BaseHealth: `1`
- MoveSpeed: `2.3`
- ZigZagAmplitude: `1.5`
- ZigZagFrequency: `2.5`
- ScoreValue: `140`

### C) Shooter Enemy (`Enemy_Shooter.prefab`)
Components:
- `SpriteRenderer`
- `Collider2D`
- `EnemyController`

Child object:
- `FirePoint` at lower nose (e.g., y = -0.6)

EnemyController settings:
- Type: `Shooter`
- BaseHealth: `2`
- MoveSpeed: `1.9`
- ShootInterval: `1.3`
- EnemyBulletPrefab: assign enemy bullet prefab
- FirePoint: assign child transform
- ScoreValue: `200`

## 3) Bullet Prefabs

### A) Player Bullet (`Bullet_Player.prefab`)
Components:
- `SpriteRenderer`
- `Collider2D` (trigger ON)
- `BulletController`

BulletController settings:
- Owner: `Player`
- Speed: `12`
- Damage: `1`
- LifeTime: `4`

### B) Enemy Bullet (`Bullet_Enemy.prefab`)
Components:
- `SpriteRenderer`
- `Collider2D` (trigger ON)
- `BulletController`

BulletController settings:
- Owner: `Enemy`
- Speed: `8`
- Damage: `1`
- LifeTime: `5`

## 4) Power-Up Prefabs

Common components:
- `SpriteRenderer`
- `Collider2D` (trigger ON)
- `PowerUpController`

### A) `PowerUp_RapidFire.prefab`
- Type: `RapidFire`
- MoveSpeed: `2`

### B) `PowerUp_Shield.prefab`
- Type: `Shield`
- MoveSpeed: `2`

### C) `PowerUp_Health.prefab`
- Type: `Health`
- MoveSpeed: `2`

## 5) Manager Object Assignments

### SpawnManager
Assign references:
- Chaser Enemy Prefab
- ZigZag Enemy Prefab
- Shooter Enemy Prefab
- RapidFire PowerUp Prefab
- Shield PowerUp Prefab
- Health PowerUp Prefab

### PlayerController
Assign references:
- Player Bullet Prefab
- Fire Point

### Enemy Shooter Prefab
Assign:
- Enemy Bullet Prefab
- Fire Point
