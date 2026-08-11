using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// A single enemy ship. Moves straight down, optionally fires bullets at a fixed
    /// interval, awards score when destroyed by the player, and despawns off-screen.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Downward speed in world units per second.")]
        [SerializeField] private float moveSpeed = 3f;

        [Header("Scoring")]
        [Tooltip("Points awarded when the player destroys this enemy.")]
        [SerializeField] private int scoreValue = 10;

        [Header("Shooting (optional)")]
        [Tooltip("If true, the enemy periodically fires bullets downward.")]
        [SerializeField] private bool canShoot = true;

        [Tooltip("Bullet prefab used by this enemy.")]
        [SerializeField] private GameObject enemyBulletPrefab;

        [Tooltip("Seconds between enemy shots.")]
        [SerializeField] private float fireInterval = 1.5f;

        [Tooltip("Downward speed for enemy bullets.")]
        [SerializeField] private float bulletSpeed = 6f;

        [Header("Effects")]
        [Tooltip("Optional explosion prefab spawned on death (particle/sprite).")]
        [SerializeField] private GameObject explosionPrefab;

        private Camera mainCamera;
        private float nextFireTime;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Start()
        {
            // Randomize the first shot so a wave of enemies does not fire in unison.
            nextFireTime = Time.time + Random.Range(0.3f, fireInterval);
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                return;
            }

            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);

            if (canShoot && enemyBulletPrefab != null && Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + fireInterval;
            }

            DespawnIfBelowScreen();
        }

        private void Fire()
        {
            Vector3 spawnPos = transform.position + Vector3.down * 0.6f;
            GameObject bulletObj = Instantiate(enemyBulletPrefab, spawnPos, Quaternion.identity);

            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Initialize(Vector2.down * bulletSpeed, Bullet.Owner.Enemy);
            }
        }

        private void DespawnIfBelowScreen()
        {
            if (mainCamera == null)
            {
                return;
            }

            Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);
            if (viewPos.y < -0.2f)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Destroy this enemy. <paramref name="awardScore"/> is true when killed by a
        /// player bullet (grants points), false on ramming collisions.
        /// </summary>
        public void DestroyEnemy(bool awardScore)
        {
            if (awardScore && GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }

            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayExplosion();
            }

            Destroy(gameObject);
        }
    }
}
