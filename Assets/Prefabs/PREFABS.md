# Prefabs

Unity creates prefab assets (`.prefab` files) when you drag a configured
GameObject from the scene Hierarchy into this folder. They can't be hand-authored
here, so this file lists exactly which prefabs to create and how to configure each.

See **"Step 4: Create the Prefabs"** in the root `README.md` for full steps.

## Prefabs to create

### 1. PlayerBullet
- SpriteRenderer (small thin sprite, e.g. cyan)
- BoxCollider2D / CircleCollider2D with **Is Trigger = true**
- `Bullet` component (direction set at runtime by the shooter)
- Tag: *(none required)*

### 2. EnemyBullet
- Same as PlayerBullet but a different color (e.g. red)
- `Bullet` component

### 3. Enemy
- SpriteRenderer (enemy ship sprite)
- Collider2D with **Is Trigger = true**
- Rigidbody2D (Body Type = Kinematic, Gravity Scale = 0)
- `Enemy` component
- `HealthSystem` component (maxHealth ~50)
- **Tag = "Enemy"**

### 4. PowerUp_Health, PowerUp_RapidFire, PowerUp_Shield
- SpriteRenderer (distinct color/icon per type)
- Collider2D with **Is Trigger = true**
- `PowerUp` component with the matching `type` selected

### 5. Player (optional prefab; can also live only in the scene)
- SpriteRenderer (player ship sprite)
- Collider2D with **Is Trigger = true**
- Rigidbody2D (Kinematic, Gravity Scale = 0)
- `PlayerController` + `HealthSystem`
- **Tag = "Player"**
