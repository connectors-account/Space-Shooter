using UnityEngine;

namespace SpaceShooter.Combat
{
    /// <summary>
    /// Handles ship-to-ship and player-to-power-up collisions.
    /// </summary>
    public class CollisionHandler : MonoBehaviour
    {
        public enum CollisionRole
        {
            Player,
            Enemy
        }

        [SerializeField] private CollisionRole role;
        [SerializeField] private int contactDamage = 15;
        [SerializeField] private MonoBehaviour damageableReference;

        private IDamageable selfDamageable;

        private void Awake()
        {
            selfDamageable = damageableReference as IDamageable;
            if (selfDamageable == null)
            {
                selfDamageable = GetComponent<IDamageable>();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (role == CollisionRole.Player)
            {
                HandlePlayerCollisions(other);
            }
            else
            {
                HandleEnemyCollisions(other);
            }
        }

        private void HandlePlayerCollisions(Collider2D other)
        {
            // Enemy body collision damages player and usually destroys enemy.
            Enemy.EnemyAI enemy = other.GetComponent<Enemy.EnemyAI>();
            if (enemy != null && selfDamageable != null)
            {
                selfDamageable.ReceiveDamage(enemy.ContactDamage, enemy.gameObject);

                var enemyDamageable = enemy.GetComponent<IDamageable>();
                enemyDamageable?.ReceiveDamage(9999, gameObject);
                return;
            }

            // Power-up pickup.
            Systems.PowerUpPickup pickup = other.GetComponent<Systems.PowerUpPickup>();
            if (pickup != null)
            {
                pickup.Consume(gameObject);
            }
        }

        private void HandleEnemyCollisions(Collider2D other)
        {
            // If enemy collides with player directly, damage both.
            Player.PlayerHealth playerHealth = other.GetComponent<Player.PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.ReceiveDamage(contactDamage, gameObject);
                selfDamageable?.ReceiveDamage(9999, other.gameObject);
            }
        }
    }
}
