using UnityEngine;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Controls player ship movement, shooting, health, and power-up states.
    /// Attach to the Player ship GameObject.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float smoothTime = 0.06f;

        [Header("Boundaries")]
        [SerializeField] private bool useScreenBoundsUtility = true;
        [SerializeField] private float minX = -8.5f;
        [SerializeField] private float maxX = 8.5f;
        [SerializeField] private float minY = -4.5f;
        [SerializeField] private float maxY = 4.5f;

        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float invincibilityDuration = 1.2f;

        [Header("Shooting Settings")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.25f;
        [SerializeField] private float rapidFireRate = 0.1f;
        [SerializeField] private float rapidFireDuration = 5f;
        [SerializeField] private float rapidFireSpreadAngle = 12f;

        [Header("Shield Settings")]
        [SerializeField] private float shieldDuration = 8f;
        [SerializeField] private GameObject shieldVisual;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color damageFlashColor = Color.red;
        [SerializeField] private float flashDuration = 0.08f;

        private int currentHealth;
        private float nextFireTime;
        private float currentFireRate;

        private bool isRapidFireActive;
        private float rapidFireEndTime;

        private bool isShieldActive;
        private float shieldEndTime;

        private bool isInvincible;
        private float invincibilityEndTime;

        private bool isDead;
        private Color originalColor;

        // Movement smoothing
        private Vector2 currentVelocity;
        private Vector2 smoothDampVelocity;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsShieldActive => isShieldActive;
        public bool IsRapidFireActive => isRapidFireActive;
        public bool IsDead => isDead;

        public event System.Action<int, int> OnHealthChanged;
        public event System.Action OnPlayerDeath;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void Start()
        {
            currentHealth = maxHealth;
            currentFireRate = fireRate;
            isDead = false;
            originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

            if (shieldVisual != null)
            {
                shieldVisual.SetActive(false);
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Update()
        {
            if (isDead) return;
            if (Managers.GameManager.Instance != null && Managers.GameManager.Instance.CurrentState != Managers.GameManager.GameState.Playing) return;

            HandleMovement();
            HandleShooting();
            HandlePowerUpTimers();
            HandleInvincibilityVisuals();
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector2 targetVelocity = new Vector2(horizontal, vertical).normalized * moveSpeed;
            currentVelocity = Vector2.SmoothDamp(currentVelocity, targetVelocity, ref smoothDampVelocity, smoothTime);

            Vector3 newPosition = transform.position + (Vector3)currentVelocity * Time.deltaTime;

            if (useScreenBoundsUtility && Utils.ScreenBounds.Instance != null)
            {
                newPosition = Utils.ScreenBounds.Instance.ClampToBounds(newPosition, 0.45f);
            }
            else
            {
                newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
                newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
            }

            transform.position = newPosition;
        }

        private void HandleShooting()
        {
            if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
            {
                FireBulletPattern();
                nextFireTime = Time.time + currentFireRate;
            }
        }

        private void FireBulletPattern()
        {
            if (bulletPrefab == null || firePoint == null) return;

            // Base shot (always)
            SpawnPlayerBullet(firePoint.position, Vector2.up);

            // Rapid fire upgrades to 3-way spread
            if (isRapidFireActive)
            {
                Vector2 leftDir = Quaternion.Euler(0f, 0f, rapidFireSpreadAngle) * Vector2.up;
                Vector2 rightDir = Quaternion.Euler(0f, 0f, -rapidFireSpreadAngle) * Vector2.up;

                SpawnPlayerBullet(firePoint.position + Vector3.left * 0.18f, leftDir);
                SpawnPlayerBullet(firePoint.position + Vector3.right * 0.18f, rightDir);
            }

            Managers.AudioManager.Instance?.PlayShootSound();
        }

        private void SpawnPlayerBullet(Vector3 spawnPosition, Vector2 direction)
        {
            GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
            bullet.tag = "PlayerBullet";

            Weapons.BulletController bulletController = bullet.GetComponent<Weapons.BulletController>();
            if (bulletController != null)
            {
                bulletController.SetDirection(direction);
            }
        }

        public void TakeDamage(int damage)
        {
            if (isDead || isInvincible || isShieldActive) return;

            currentHealth = Mathf.Max(0, currentHealth - damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            StartCoroutine(DamageFlash());

            isInvincible = true;
            invincibilityEndTime = Time.time + invincibilityDuration;

            Managers.AudioManager.Instance?.PlayPlayerHitSound();

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (isDead) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Die()
        {
            isDead = true;
            OnPlayerDeath?.Invoke();

            Managers.AudioManager.Instance?.PlayExplosionSound();

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }
        }

        public void ActivateRapidFire()
        {
            isRapidFireActive = true;
            currentFireRate = rapidFireRate;
            rapidFireEndTime = Time.time + rapidFireDuration;
        }

        public void ActivateShield()
        {
            isShieldActive = true;
            shieldEndTime = Time.time + shieldDuration;

            if (shieldVisual != null)
            {
                shieldVisual.SetActive(true);
            }
        }

        private void HandlePowerUpTimers()
        {
            if (isRapidFireActive && Time.time >= rapidFireEndTime)
            {
                isRapidFireActive = false;
                currentFireRate = fireRate;
            }

            if (isShieldActive && Time.time >= shieldEndTime)
            {
                isShieldActive = false;
                if (shieldVisual != null)
                {
                    shieldVisual.SetActive(false);
                }
            }
        }

        private void HandleInvincibilityVisuals()
        {
            if (isInvincible && Time.time >= invincibilityEndTime)
            {
                isInvincible = false;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
            }

            if (isInvincible && spriteRenderer != null)
            {
                float alpha = Mathf.PingPong(Time.time * 11f, 1f) > 0.5f ? 1f : 0.35f;
                Color c = originalColor;
                c.a = alpha;
                spriteRenderer.color = c;
            }
        }

        private System.Collections.IEnumerator DamageFlash()
        {
            if (spriteRenderer == null) yield break;

            Color previous = spriteRenderer.color;
            spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(flashDuration);

            if (!isInvincible)
            {
                spriteRenderer.color = previous;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isDead) return;

            if (other.CompareTag("EnemyBullet"))
            {
                Weapons.BulletController bullet = other.GetComponent<Weapons.BulletController>();
                int damage = bullet != null ? bullet.Damage : 10;
                TakeDamage(damage);
                Destroy(other.gameObject);
                return;
            }

            if (other.CompareTag("Enemy"))
            {
                TakeDamage(20);

                Enemy.EnemyController enemy = other.GetComponent<Enemy.EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(9999); // Ram collision destroys enemy.
                }

                return;
            }

            if (other.CompareTag("PowerUp"))
            {
                PowerUps.PowerUpController powerUp = other.GetComponent<PowerUps.PowerUpController>();
                if (powerUp != null)
                {
                    powerUp.ApplyEffect(this);
                    Destroy(other.gameObject);
                }
            }
        }

        public void ResetPlayer()
        {
            isDead = false;
            currentHealth = maxHealth;
            currentFireRate = fireRate;

            isRapidFireActive = false;
            isShieldActive = false;
            isInvincible = false;

            currentVelocity = Vector2.zero;
            smoothDampVelocity = Vector2.zero;

            transform.position = new Vector3(0f, -3.8f, 0f);

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.color = originalColor;
            }

            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = true;
            }

            if (shieldVisual != null)
            {
                shieldVisual.SetActive(false);
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
