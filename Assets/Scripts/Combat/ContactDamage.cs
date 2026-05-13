// ============================================================================
// ContactDamage.cs — Attach to enemy prefabs so colliding with the player
// deals damage (and optionally destroys the enemy).
// ============================================================================
using UnityEngine;

namespace SpaceShooter.Combat
{
    public class ContactDamage : MonoBehaviour
    {
        [SerializeField] private int damageToPlayer = 20;
        [SerializeField] private bool destroySelfOnContact = true;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var playerHealth = other.GetComponent<Player.PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
            }

            if (destroySelfOnContact)
            {
                var enemy = GetComponent<Enemies.EnemyBase>();
                if (enemy != null)
                {
                    // Use TakeDamage with lethal amount so normal death flow triggers
                    enemy.TakeDamage(9999);
                }
            }
        }
    }
}
