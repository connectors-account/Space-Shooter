using SpaceShooter.Core;
using SpaceShooter.Enemies;
using SpaceShooter.Player;
using SpaceShooter.PowerUps;
using SpaceShooter.Weapons;
using UnityEngine;

namespace SpaceShooter.Managers
{
    /// <summary>
    /// Centralises all collision resolution logic. Active colliders (bullets and the player ship)
    /// detect physics triggers and delegate the outcome to this class so the rules for
    /// bullet-vs-enemy, bullet-vs-player, player-vs-enemy and player-vs-power-up live in one place.
    /// </summary>
    public static class CollisionHandler
    {
        /// <summary>
        /// Resolves a bullet striking another collider. The bullet only damages objects of the
        /// opposing faction. Returns true if the bullet was consumed and should be recycled.
        /// </summary>
        /// <param name="bullet">The bullet that triggered the collision.</param>
        /// <param name="other">The collider the bullet overlapped.</param>
        public static bool HandleBulletHit(Bullet bullet, Collider2D other)
        {
            if (bullet == null || other == null)
            {
                return false;
            }

            IDamageable target = other.GetComponent<IDamageable>();
            if (target == null || target.IsDead)
            {
                return false;
            }

            // Bullets never damage their own faction.
            if (target.Faction == bullet.Faction)
            {
                return false;
            }

            target.TakeDamage(bullet.Damage);
            return true;
        }

        /// <summary>
        /// Resolves the player ship overlapping another collider: contact damage from enemies and
        /// collection of power-ups.
        /// </summary>
        /// <param name="player">The player ship.</param>
        /// <param name="other">The overlapped collider.</param>
        public static void HandlePlayerContact(PlayerController player, Collider2D other)
        {
            if (player == null || other == null || player.IsDead)
            {
                return;
            }

            // Power-up pickup.
            PowerUp powerUp = other.GetComponent<PowerUp>();
            if (powerUp != null)
            {
                player.ApplyPowerUp(powerUp.Type);
                powerUp.Collect();
                return;
            }

            // Contact with an enemy body damages both.
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                player.TakeDamage(enemy.ContactDamage);
                // Non-boss enemies are destroyed on contact; bosses shrug it off.
                if (enemy.Type != EnemyType.Boss)
                {
                    enemy.TakeDamage(enemy.MaxHealth);
                }
            }
        }
    }
}
