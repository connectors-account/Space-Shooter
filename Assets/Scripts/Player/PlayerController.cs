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
        [SerializeField] private float smoothTime = 0.05f;

        [Header("Boundaries (Viewport)")]
        [SerializeField] private float minX = -8.5f;
        [SerializeField] private float maxX = 8.5f;
        [SerializeField] private float minY = -4.5f;
        [SerializeField] private float maxY = 4.5f;

        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float invincibilityDuration = 1.5f;

        [Header("Shooting Settings")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.25f;
        [SerializeField] private float rapidFireRate = 0.1f;
        [SerializeField] private float rapidFireDuration = 5f;

        [Header("Shield Settings")]
        [SerializeField] private float shieldDuration = 8f;
        [SerializeField] private GameObject shieldVisual; // child object for shield effect

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color damageFlashColor = Color.red;
        [SerializeField] private float flashDuration = 0.1f;

        // ---- Runtime State ----
        private int currentHealth;
        private float nextFireTime;
        private float currentFireRate;
        private bool isRapidFireActive;
        private float rapidFireEndTime;
        private bool isShieldActive;
        private float shieldEndTime;
        private bool isInvincible;
        private float invincibilityEndTime;
        private Vector2 velocity;
        private Color originalColor;
        private bool isDead;

        // ---- Public Properties ----
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsShieldActive => isShieldActive;
        public bool IsRapidFireActive => isRapidFireActive;
        public bool IsDead => isDead;

        // ---- Events ----
        public event System.Action<int, int> OnHealthChanged;  // currentHP, maxHP
        public event System.Action OnPlayerDeath;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            currentHealth = maxHealth;
            currentFireRate = fireRate;
            isDead = false;

            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;

            if (shieldVisual != null)
                shieldVisual.SetActive(false);

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Update()
        {
            if (isDead) return;

            HandleMovement();
            HandleShooting();
            HandlePowerUpTimers();
            HandleInvincibility();
        }

        // ========== MOVEMENT ==========
        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector2 targetVelocity = new Vector2(horizontal, vertical).normalized * moveSpeed;
            Vector2 currentVel = velocity;
            velocity = Vector2.SmoothDamp(velocity, targetVelocity, ref currentVel, smoothTime);

            Vector3 newPosition = transform.position + (Vector3)velocity * Time.deltaTime;

            // Clamp to screen boundaries
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

            transform.position = newPosition;
        }

        // ========== SHOOTING ==========
        private void HandleShooting()
        {
            if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
            {
                FireBullet();
                nextFireTime = Time.time + currentFireRate;
            }
        }

        private void FireBullet()
        {
            if (bulletPrefab == null || firePoint == null) return;

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            // Tag bullet as player bullet
            bullet.tag = "PlayerBullet";

            // Play shoot sound
            Managers.AudioManager.Instance?.PlayShootSound();
        }

        // ========== HEALTH & DAMAGE ==========
        public void TakeDamage(int damage)
        {
            if (isDead || isInvincible || isShieldActive) return;

            currentHealth -= damage;
            currentHealth = Mathf.Max(currentHealth, 0);

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Visual feedback
            StartCoroutine(DamageFlash());

            // Brief invincibility after being hit
            isInvincible = true;
            invincibilityEndTime = Time.time + invincibilityDuration;

            Managers.AudioManager.Instance?.PlayExplosionSound();

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (isDead) return;

            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Die()
        {
            isDead = true;
            OnPlayerDeath?.Invoke();

            Managers.AudioManager.Instance?.PlayExplosionSound();

            // Disable the sprite but keep the object alive for game-over logic
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;

            // Disable collider
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        // ========== POWER-UPS ==========
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
                shieldVisual.SetActive(true);
        }

        private void HandlePowerUpTimers()
        {
            // Rapid fire timer
            if (isRapidFireActive && Time.time >= rapidFireEndTime)
            {
                isRapidFireActive = false;
                currentFireRate = fireRate;
            }

            // Shield timer
            if (isShieldActive && Time.time >= shieldEndTime)
            {
                isShieldActive = false;
                if (shieldVisual != null)
                    shieldVisual.SetActive(false);
            }
        }

        private void HandleInvincibility()
        {
            if (isInvincible && Time.time >= invincibilityEndTime)
            {
                isInvincible = false;
                if (spriteRenderer != null)
                    spriteRenderer.color = originalColor;
            }

            // Blink effect while invincible
            if (isInvincible && spriteRenderer != null)
            {
                float alpha = Mathf.PingPong(Time.time * 10f, 1f) > 0.5f ? 1f : 0.3f;
                Color c = originalColor;
                c.a = alpha;
                spriteRenderer.color = c;
            }
        }

        // ========== VISUAL EFFECTS ==========
        private System.Collections.IEnumerator DamageFlash()
        {
            if (spriteRenderer == null) yield break;

            spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }

        // ========== COLLISION ==========
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isDead) return;

            // Collide with enemy bullets
            if (other.CompareTag("EnemyBullet"))
            {
                Weapons.BulletController bullet = other.GetComponent<Weapons.BulletController>();
                if (bullet != null)
                {
                    TakeDamage(bullet.Damage);
                    Destroy(other.gameObject);
                }
            }

            // Collide with enemies directly
            if (other.CompareTag("Enemy"))
            {
                TakeDamage(20);
            }

            // Collide with power-ups
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

        /// <summary>
        /// Resets the player to initial state (for restarting the game).
        /// </summary>
        public void ResetPlayer()
        {
            isDead = false;
            currentHealth = maxHealth;
            currentFireRate = fireRate;
            isRapidFireActive = false;
            isShieldActive = false;
            isInvincible = false;
            transform.position = new Vector3(0, -3f, 0);

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.color = originalColor;
            }

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            if (shieldVisual != null)
                shieldVisual.SetActive(false);

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
