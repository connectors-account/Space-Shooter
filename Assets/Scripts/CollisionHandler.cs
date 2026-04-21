using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Shared damage/collision utility used by bullets, enemies, and the player.
    /// This keeps combat rules in one place and avoids duplicated hit logic.
    /// </summary>
    public static class CollisionHandler
    {
        public static bool TryApplyDamage(Collider2D targetCollider, int damage, DamageTeam sourceTeam)
        {
            if (targetCollider == null || damage <= 0)
            {
                return false;
            }

            // Ignore world boundaries/triggers that are not damageable.
            Component damageableComponent = targetCollider.GetComponent(typeof(IDamageable));
            if (damageableComponent == null)
            {
                return false;
            }

            IDamageable damageable = damageableComponent as IDamageable;
            if (damageable == null || !damageable.IsAlive)
            {
                return false;
            }

            // Prevent friendly fire for player/enemy teams.
            if (sourceTeam != DamageTeam.Neutral && damageable.Team == sourceTeam)
            {
                return false;
            }

            damageable.TakeDamage(damage, sourceTeam);
            return true;
        }
    }

    public enum DamageTeam
    {
        Player,
        Enemy,
        Neutral
    }

    public interface IDamageable
    {
        DamageTeam Team { get; }
        bool IsAlive { get; }
        void TakeDamage(int amount, DamageTeam sourceTeam);
    }
}
