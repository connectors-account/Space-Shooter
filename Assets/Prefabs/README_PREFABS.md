# Prefab Setup Guide

Create these prefabs in Unity by following the steps below.

---

## 1. Player Ship Prefab (`Player.prefab`)

1. **Create** → right-click Hierarchy → Create Empty → name it "Player"
2. **Add Components:**
   - `SpriteRenderer` → assign player_ship sprite (or Unity Square sprite)
   - `Rigidbody2D` → Gravity Scale = 0, Freeze Rotation Z = ✓
   - `BoxCollider2D` → Is Trigger = ✓, adjust size to sprite
   - `PlayerController` (script)
   - `HealthSystem` (script) → Max Health = 100, Destroy On Death = ✓
   - `AudioSource`
3. **Create child** → empty child named "FirePoint" at (0, 0.6, 0)
4. **Tag** = "Player", **Layer** = "Player"
5. **Configure PlayerController:**
   - Bullet Prefab = PlayerBullet prefab (create below first)
   - Fire Point = the FirePoint child transform
6. **Drag** from Hierarchy into `Assets/Prefabs/` to create prefab

---

## 2. Player Bullet Prefab (`PlayerBullet.prefab`)

1. Create Empty → name "PlayerBullet"
2. **Add Components:**
   - `SpriteRenderer` → assign bullet sprite (or small Square), color = cyan
   - `Rigidbody2D` → Gravity Scale = 0
   - `BoxCollider2D` → Is Trigger = ✓, size ~ (0.1, 0.3)
   - `BulletController` (script) → Speed = 15, Damage = 25
3. **Tag** = "PlayerBullet", **Layer** = "PlayerBullet"
4. Scale to (0.15, 0.4, 1) for elongated bullet look
5. **Drag** into `Assets/Prefabs/`

---

## 3. Enemy Bullet Prefab (`EnemyBullet.prefab`)

1. Duplicate PlayerBullet, rename to "EnemyBullet"
2. Change color to red
3. **Tag** = "EnemyBullet", **Layer** = "EnemyBullet"
4. **Drag** into `Assets/Prefabs/`

---

## 4. Enemy Prefabs (`Enemy_Basic.prefab`, `Enemy_Zigzag.prefab`, `Enemy_Dive.prefab`)

For each enemy type:
1. Create Empty → name appropriately
2. **Add Components:**
   - `SpriteRenderer` → assign enemy sprite, color varies by type
   - `Rigidbody2D` → Gravity Scale = 0, Freeze Rotation Z = ✓
   - `BoxCollider2D` → Is Trigger = ✓
   - `EnemyController` (script)
   - `HealthSystem` (script) → Max Health varies (50/75/100)
3. **Create child** "FirePoint" at (0, -0.5, 0)
4. **Tag** = "Enemy", **Layer** = "Enemy"
5. **Configure EnemyController:**
   - Bullet Prefab = EnemyBullet prefab
   - Fire Point = child FirePoint
   - Score Value = 100/150/200
   - Pattern = StraightDown/Zigzag/Dive (respectively)
   - Possible Drops = array of PowerUp prefabs
   - Drop Chance = 0.15
6. **Drag** each into `Assets/Prefabs/`

---

## 5. Power-Up Prefabs

Create one for each type (WeaponUpgrade, Shield, HealthRestore, RapidFire, ScoreBonus):
1. Create Empty → name "PowerUp_Weapon" (etc.)
2. **Add Components:**
   - `SpriteRenderer` → assign sprite, script auto-tints by type
   - `Rigidbody2D` → Gravity Scale = 0
   - `CircleCollider2D` → Is Trigger = ✓, Radius = 0.3
   - `PowerUpController` (script) → set Type dropdown accordingly
3. **Tag** = "PowerUp", **Layer** = "PowerUp"
4. Scale to (0.5, 0.5, 1)
5. **Drag** each into `Assets/Prefabs/`
