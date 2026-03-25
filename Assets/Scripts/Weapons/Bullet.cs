// =============================================================================
// Bullet.cs — Universal projectile for player and enemy bullets
// =============================================================================
using UnityEngine;

namespace SpaceShooter.Weapons
{
    /// <summary>
    /// Bullet projectile that moves in a specified direction at a given speed.
    /// Self-destructs when off-screen or after a timeout.
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        [Header("Defaults (overridden by Initialize)")]
        [SerializeField] private float defaultSpeed = 10f;
        [SerializeField] private int defaultDamage = 1;
        [SerializeField] private float lifetime = 5f;

        private Vector2 direction = Vector2.up;
        private float speed;
        private bool isPlayerBullet = true;
        private int damage;
        private float spawnTime;

        /// <summary>Whether this bullet belongs to the player.</summary>
        public bool IsPlayerBullet => isPlayerBullet;

        /// <summary>Damage this bullet inflicts.</summary>
        public int Damage => damage;

        private void Awake()
        {
            speed = defaultSpeed;
            damage = defaultDamage;
            spawnTime = Time.time;
        }

        /// <summary>
        /// Initializes bullet parameters. Called by the firing entity.
        /// </summary>
        public void Initialize(Vector2 dir, float spd, bool playerBullet, int dmg)
        {
            direction = dir.normalized;
            speed = spd;
            isPlayerBullet = playerBullet;
            damage = dmg;

            // Rotate sprite to face movement direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Set tag for collision filtering
            gameObject.tag = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";
        }

        private void Update()
        {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);

            // Destroy after lifetime
            if (Time.time - spawnTime > lifetime)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Destroys bullet if it leaves the visible screen area.
        /// </summary>
        private void OnBecameInvisible()
        {
            Destroy(gameObject);
        }
    }
}
