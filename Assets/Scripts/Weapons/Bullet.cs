using UnityEngine;

namespace SpaceShooter.Weapons
{
    /// <summary>
    /// Generic bullet behaviour used by both player and enemy projectiles.
    /// Managed via object pooling - returns to pool on disable.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float speed = 12f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private int damage = 1;

        private Vector2 direction;
        private float timer;
        private bool isPlayerBullet;
        private Rigidbody2D rb;

        public int Damage => damage;
        public bool IsPlayerBullet => isPlayerBullet;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        /// <summary>
        /// Initialise bullet direction and ownership after spawning from pool.
        /// </summary>
        public void Initialize(Vector2 dir, bool playerBullet, float customSpeed = -1f)
        {
            direction = dir.normalized;
            isPlayerBullet = playerBullet;
            timer = lifetime;

            if (customSpeed > 0f)
                speed = customSpeed;

            // Set tag based on ownership
            gameObject.tag = playerBullet ? "PlayerBullet" : "EnemyBullet";

            rb.linearVelocity = direction * speed;
        }

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                ReturnToPool();
                return;
            }

            // Off-screen check
            Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
            if (vp.x < -0.1f || vp.x > 1.1f || vp.y < -0.1f || vp.y > 1.1f)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isPlayerBullet && other.CompareTag("Enemy"))
            {
                Enemies.EnemyBase enemy = other.GetComponent<Enemies.EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                ReturnToPool();
            }
            else if (!isPlayerBullet && other.CompareTag("Player"))
            {
                // Damage handled by PlayerController.OnTriggerEnter2D
                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            rb.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }
    }
}
