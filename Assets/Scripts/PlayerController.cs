using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Controls the player ship: movement (WASD / arrow keys), clamping to the
    /// visible screen area, shooting straight bullets (Space), and taking damage.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Movement speed in world units per second.")]
        [SerializeField] private float moveSpeed = 8f;

        [Tooltip("Padding kept from the screen edges so the ship stays fully visible.")]
        [SerializeField] private float edgePadding = 0.5f;

        [Header("Shooting")]
        [Tooltip("Bullet prefab to spawn when firing.")]
        [SerializeField] private GameObject bulletPrefab;

        [Tooltip("Point from which bullets spawn. Defaults to slightly above the ship.")]
        [SerializeField] private Transform firePoint;

        [Tooltip("Minimum seconds between shots.")]
        [SerializeField] private float fireCooldown = 0.25f;

        [Tooltip("Upward speed given to fired bullets.")]
        [SerializeField] private float bulletSpeed = 12f;

        [Header("Damage")]
        [Tooltip("Seconds of invulnerability after being hit.")]
        [SerializeField] private float invulnerabilityTime = 1.0f;

        [Tooltip("Sprite renderer used for the hit-flash effect. Auto-found if empty.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Camera mainCamera;
        private float nextFireTime;
        private float invulnerableUntil;

        private void Awake()
        {
            mainCamera = Camera.main;
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                return;
            }

            HandleMovement();
            HandleShooting();
            HandleInvulnerabilityFlash();
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 direction = new Vector3(horizontal, vertical, 0f).normalized;
            Vector3 newPosition = transform.position + direction * moveSpeed * Time.deltaTime;

            if (mainCamera != null)
            {
                Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
                Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

                newPosition.x = Mathf.Clamp(newPosition.x, min.x + edgePadding, max.x - edgePadding);
                newPosition.y = Mathf.Clamp(newPosition.y, min.y + edgePadding, max.y - edgePadding);
            }

            newPosition.z = 0f;
            transform.position = newPosition;
        }

        private void HandleShooting()
        {
            if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + fireCooldown;
            }
        }

        private void Fire()
        {
            if (bulletPrefab == null)
            {
                return;
            }

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.6f;
            GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                // Player bullets travel straight up and only hurt enemies.
                bullet.Initialize(Vector2.up * bulletSpeed, Bullet.Owner.Player);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayShoot();
            }
        }

        private void HandleInvulnerabilityFlash()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            bool invulnerable = Time.time < invulnerableUntil;
            if (invulnerable)
            {
                // Blink roughly 10 times per second.
                float alpha = Mathf.PingPong(Time.time * 10f, 1f) * 0.7f + 0.3f;
                Color c = spriteRenderer.color;
                c.a = alpha;
                spriteRenderer.color = c;
            }
            else
            {
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }
        }

        /// <summary>Apply one point of damage to the player, respecting invulnerability.</summary>
        public void TakeDamage()
        {
            if (Time.time < invulnerableUntil)
            {
                return;
            }

            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                return;
            }

            invulnerableUntil = Time.time + invulnerabilityTime;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerHit();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Direct collision with an enemy body damages the player and destroys the enemy.
            if (other.CompareTag("Enemy"))
            {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.DestroyEnemy(false);
                }
                TakeDamage();
            }
        }
    }
}
