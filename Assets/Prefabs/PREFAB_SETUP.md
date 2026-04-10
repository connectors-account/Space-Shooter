# Prefab Creation Checklist

Create these prefabs in Unity and save each under `Assets/Prefabs/`:

1. **Player.prefab**
   - Components: `SpriteRenderer`, `BoxCollider2D (IsTrigger)`, `Rigidbody2D (Kinematic)`, `SpaceShooter.Player.PlayerController`
   - Child objects: `FirePoint`, optional `ShieldVisual`

2. **PlayerBullet.prefab**
   - Components: `SpriteRenderer`, `BoxCollider2D (IsTrigger)`, `Rigidbody2D (Kinematic)`, `SpaceShooter.Weapons.BulletController`
   - Tag: `PlayerBullet`

3. **EnemyBullet.prefab**
   - Same setup as player bullet, tag `EnemyBullet`

4. **EnemyBasic.prefab**
   - Components: `SpriteRenderer`, `BoxCollider2D (IsTrigger)`, `Rigidbody2D (Kinematic)`, `SpaceShooter.Enemy.EnemyController`
   - Tag: `Enemy`
   - Child object: `FirePoint`

5. **EnemyZigzag.prefab**
   - Same as EnemyBasic, set Enemy Type = Zigzag

6. **EnemyTank.prefab**
   - Same as EnemyBasic, set Enemy Type = Tank

7. **PowerUpHealth.prefab**
   - Components: `SpriteRenderer`, `CircleCollider2D (IsTrigger)`, `Rigidbody2D (Kinematic)`, `SpaceShooter.PowerUps.PowerUpController`
   - Tag: `PowerUp`
   - Type: `HealthPack`

8. **PowerUpRapidFire.prefab**
   - Same as power-up health, type `RapidFire`

9. **PowerUpShield.prefab**
   - Same as power-up health, type `Shield`

10. **ExplosionParticles.prefab** (optional)
   - Empty object with `SpaceShooter.Effects.ExplosionParticles`
   - Runtime spawning is already handled by scripts, so this prefab is optional.
