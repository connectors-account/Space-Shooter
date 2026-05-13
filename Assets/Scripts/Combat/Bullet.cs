// ============================================================================
// Bullet.cs — Universal bullet behaviour for both player and enemy projectiles
// Direction, speed, damage, and pool tag are configured per-prefab.
// ============================================================================
using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Combat
{
    public class Bullet : MonoBehaviour
    {
        [Header("Bullet Settings")]
        [SerializeField] private float speed = 12f;
        [SerializeField] private int damage = 1;
        [SerializeField] private bool isPlayerBullet = true;      // false = enemy bullet
        [SerializeField] private string poolTag = "PlayerBullet"; // for returning to pool

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            // Move in the bullet's local up direction (set by rotation at spawn)
            transform.Translate(Vector3.up * (speed * Time.deltaTime));

            // Recycle when off-screen
            if (GameBounds.Instance != null && GameBounds.Instance.IsOutOfBounds(transform.position))
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isPlayerBullet)
            {
                // Player bullets hit enemies
                var enemy = other.GetComponent<Enemies.EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    ReturnToPool();
                }
            }
            else
            {
                // Enemy bullets hit player
                if (other.CompareTag("Player"))
                {
                    var playerHealth = other.GetComponent<Player.PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(damage * 10);  // scale for player HP pool
                        ReturnToPool();
                    }
                }
            }
        }

        private void ReturnToPool()
        {
            if (ObjectPool.Instance != null)
                ObjectPool.Instance.ReturnToPool(poolTag, gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}
