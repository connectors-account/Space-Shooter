using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Controls the player ship: movement (WASD / Arrow keys), shooting (Space)
    /// with a fire cooldown, weapon upgrade levels, shield handling and death.
    /// Movement is clamped to the visible camera bounds.
    /// </summary>
    [RequireComponent(typeof(HealthSystem))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float padding = 0.5f;

        [Header("Shooting")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform[] firePoints;
        [SerializeField] private float fireCooldown = 0.25f;
        [SerializeField] private float bulletSpeed = 14f;
        [SerializeField] private float bulletDamage = 25f;

        [Header("Weapon Upgrade")]
        [SerializeField] private int weaponLevel = 1;
        [SerializeField] private int maxWeaponLevel = 3;

        [Header("Visuals")]
        [SerializeField] private GameObject shieldVisual;

        private HealthSystem health;
        private float fireTimer;
        private Camera cam;
        private Vector2 minBounds;
        private Vector2 maxBounds;
        private bool controlsEnabled = true;

        public int WeaponLevel => weaponLevel;

        private void Awake()
        {
            health = GetComponent<HealthSystem>();
            cam = Camera.main;
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.OnDeath += HandleDeath;
                health.OnShieldChanged += HandleShieldChanged;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDeath -= HandleDeath;
                health.OnShieldChanged -= HandleShieldChanged;
            }
        }

        private void Start()
        {
            CalculateBounds();
            if (shieldVisual != null) shieldVisual.SetActive(false);
        }

        private void Update()
        {
            if (!controlsEnabled) return;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

            HandleMovement();
            HandleShooting();
        }

        private void CalculateBounds()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            Vector3 min = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
            Vector3 max = cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));
            minBounds = new Vector2(min.x + padding, min.y + padding);
            maxBounds = new Vector2(max.x - padding, max.y - padding);
        }

        private void HandleMovement()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 move = new Vector3(h, v, 0f).normalized * moveSpeed * Time.deltaTime;
            Vector3 newPos = transform.position + move;

            newPos.x = Mathf.Clamp(newPos.x, minBounds.x, maxBounds.x);
            newPos.y = Mathf.Clamp(newPos.y, minBounds.y, maxBounds.y);

            transform.position = newPos;
        }

        private void HandleShooting()
        {
            fireTimer -= Time.deltaTime;

            if (Input.GetKey(KeyCode.Space) && fireTimer <= 0f)
            {
                Shoot();
                fireTimer = fireCooldown;
            }
        }

        private void Shoot()
        {
            if (bulletPrefab == null || firePoints == null || firePoints.Length == 0) return;

            // Number of streams scales with weapon level.
            switch (weaponLevel)
            {
                case 1:
                    SpawnBullet(firePoints[0].position, Vector2.up);
                    break;
                case 2:
                    SpawnBullet(firePoints[0].position + Vector3.left * 0.25f, Vector2.up);
                    SpawnBullet(firePoints[0].position + Vector3.right * 0.25f, Vector2.up);
                    break;
                default: // level 3+: triple spread
                    SpawnBullet(firePoints[0].position, Vector2.up);
                    SpawnBullet(firePoints[0].position, new Vector2(-0.25f, 1f));
                    SpawnBullet(firePoints[0].position, new Vector2(0.25f, 1f));
                    break;
            }

            AudioManager.Instance?.PlayPlayerShoot();
        }

        private void SpawnBullet(Vector3 position, Vector2 dir)
        {
            GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
            BulletController bc = bullet.GetComponent<BulletController>();
            if (bc != null)
            {
                bc.Configure(BulletOwner.Player, dir, bulletSpeed, bulletDamage);
            }
        }

        // ---------- Power-up effects ----------

        public void UpgradeWeapon()
        {
            weaponLevel = Mathf.Min(weaponLevel + 1, maxWeaponLevel);
            AudioManager.Instance?.PlayPowerUp();
        }

        public void AddHealth(float amount)
        {
            health.Heal(amount);
            AudioManager.Instance?.PlayPowerUp();
        }

        public void ActivateShield(float duration)
        {
            health.ActivateShield(duration);
            AudioManager.Instance?.PlayPowerUp();
        }

        // ---------- State handling ----------

        private void HandleShieldChanged(bool active)
        {
            if (shieldVisual != null) shieldVisual.SetActive(active);
        }

        private void HandleDeath()
        {
            controlsEnabled = false;
            AudioManager.Instance?.PlayExplosion();
            GameManager.Instance?.OnPlayerDied();
            gameObject.SetActive(false);
        }

        public void ResetPlayer(Vector3 startPosition)
        {
            transform.position = startPosition;
            weaponLevel = 1;
            controlsEnabled = true;
            fireTimer = 0f;
            gameObject.SetActive(true);
            health.ResetHealth();
            CalculateBounds();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Direct collision with an enemy body damages the player and the enemy.
            if (other.CompareTag("Enemy"))
            {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    health.TakeDamage(enemy.CollisionDamage);
                    enemy.Die(false);
                }
            }
        }
    }
}
