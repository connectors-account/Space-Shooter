using UnityEngine;
using System;
using SpaceShooter.Managers;
using SpaceShooter.Combat;

namespace SpaceShooter.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float smoothTime = 0.05f;

        [Header("Boundaries")]
        [SerializeField] private float minX = -8f;
        [SerializeField] private float maxX = 8f;
        [SerializeField] private float minY = -4f;
        [SerializeField] private float maxY = 4f;

        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth;
        [SerializeField] private float invincibilityDuration = 1.5f;
        [SerializeField] private bool isInvincible = false;

        [Header("Shooting Settings")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private float fireRate = 0.2f;
        [SerializeField] private int weaponLevel = 1;
        [SerializeField] private int maxWeaponLevel = 5;

        [Header("Shield")]
        [SerializeField] private bool hasShield = false;
        [SerializeField] private int shieldHits = 0;
        [SerializeField] private GameObject shieldVisual;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color damageColor = Color.red;
        [SerializeField] private float flashDuration = 0.1f;

        private Rigidbody2D rb;
        private Vector2 moveInput;
        private Vector2 velocity;
        private float nextFireTime;
        private Color originalColor;

        public event Action<int, int> OnHealthChanged;
        public event Action<int> OnWeaponLevelChanged;
        public event Action OnPlayerDeath;
        public event Action<bool> OnShieldChanged;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public int WeaponLevel => weaponLevel;
        public bool HasShield => hasShield;

        private void Awake()
        {
            Instance = this;
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
        }

        private void Start()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnWeaponLevelChanged?.Invoke(weaponLevel);
            
            if (shieldVisual != null)
                shieldVisual.SetActive(false);
        }

        private void Update()
        {
            if (GameManager.Instance != null && (GameManager.Instance.IsPaused || GameManager.Instance.IsGameOver))
                return;

            HandleInput();
            HandleShooting();
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance != null && (GameManager.Instance.IsPaused || GameManager.Instance.IsGameOver))
                return;

            HandleMovement();
        }

        private void HandleInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(horizontal, vertical).normalized;
        }

        private void HandleMovement()
        {
            Vector2 targetVelocity = moveInput * moveSpeed;
            rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref velocity, smoothTime);

            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
            transform.position = clampedPosition;
        }

        private void HandleShooting()
        {
            if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + fireRate;
            }
        }

        private void Fire()
        {
            if (bulletPrefab == null || firePoint == null) return;

            AudioManager.Instance?.PlaySound("PlayerShoot");

            switch (weaponLevel)
            {
                case 1:
                    SpawnBullet(firePoint.position, Vector2.up);
                    break;
                case 2:
                    SpawnBullet(firePoint.position + Vector3.left * 0.2f, Vector2.up);
                    SpawnBullet(firePoint.position + Vector3.right * 0.2f, Vector2.up);
                    break;
                case 3:
                    SpawnBullet(firePoint.position, Vector2.up);
                    SpawnBullet(firePoint.position + Vector3.left * 0.3f, Vector2.up);
                    SpawnBullet(firePoint.position + Vector3.right * 0.3f, Vector2.up);
                    break;
                case 4:
                    SpawnBullet(firePoint.position, Vector2.up);
                    SpawnBullet(firePoint.position + Vector3.left * 0.3f, new Vector2(-0.1f, 1f).normalized);
                    SpawnBullet(firePoint.position + Vector3.right * 0.3f, new Vector2(0.1f, 1f).normalized);
                    break;
                case 5:
                    SpawnBullet(firePoint.position, Vector2.up);
                    SpawnBullet(firePoint.position + Vector3.left * 0.2f, Vector2.up);
                    SpawnBullet(firePoint.position + Vector3.right * 0.2f, Vector2.up);
                    SpawnBullet(firePoint.position + Vector3.left * 0.4f, new Vector2(-0.2f, 1f).normalized);
                    SpawnBullet(firePoint.position + Vector3.right * 0.4f, new Vector2(0.2f, 1f).normalized);
                    break;
            }
        }

        private void SpawnBullet(Vector3 position, Vector2 direction)
        {
            GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Initialize(direction, true, 10 + weaponLevel * 2);
            }
        }

        public void TakeDamage(int damage)
        {
            if (isInvincible) return;

            if (hasShield)
            {
                shieldHits--;
                AudioManager.Instance?.PlaySound("ShieldHit");
                
                if (shieldHits <= 0)
                {
                    RemoveShield();
                }
                return;
            }

            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            AudioManager.Instance?.PlaySound("PlayerHit");
            StartCoroutine(FlashDamage());
            StartCoroutine(InvincibilityFrames());

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private System.Collections.IEnumerator FlashDamage()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = damageColor;
                yield return new WaitForSeconds(flashDuration);
                spriteRenderer.color = originalColor;
            }
        }

        private System.Collections.IEnumerator InvincibilityFrames()
        {
            isInvincible = true;
            float elapsed = 0f;
            
            while (elapsed < invincibilityDuration)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = !spriteRenderer.enabled;
                }
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            
            if (spriteRenderer != null)
                spriteRenderer.enabled = true;
            
            isInvincible = false;
        }

        private void Die()
        {
            AudioManager.Instance?.PlaySound("PlayerDeath");
            EffectsManager.Instance?.SpawnExplosion(transform.position, 2f);
            OnPlayerDeath?.Invoke();
            GameManager.Instance?.GameOver();
            Destroy(gameObject);
        }

        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            AudioManager.Instance?.PlaySound("Heal");
        }

        public void UpgradeWeapon()
        {
            if (weaponLevel < maxWeaponLevel)
            {
                weaponLevel++;
                OnWeaponLevelChanged?.Invoke(weaponLevel);
                AudioManager.Instance?.PlaySound("PowerUp");
            }
        }

        public void AddShield(int hits = 3)
        {
            hasShield = true;
            shieldHits = hits;
            
            if (shieldVisual != null)
                shieldVisual.SetActive(true);
            
            OnShieldChanged?.Invoke(true);
            AudioManager.Instance?.PlaySound("PowerUp");
        }

        private void RemoveShield()
        {
            hasShield = false;
            shieldHits = 0;
            
            if (shieldVisual != null)
                shieldVisual.SetActive(false);
            
            OnShieldChanged?.Invoke(false);
        }

        public void IncreaseSpeed(float multiplier, float duration)
        {
            StartCoroutine(SpeedBoostCoroutine(multiplier, duration));
        }

        private System.Collections.IEnumerator SpeedBoostCoroutine(float multiplier, float duration)
        {
            float originalSpeed = moveSpeed;
            moveSpeed *= multiplier;
            AudioManager.Instance?.PlaySound("PowerUp");
            
            yield return new WaitForSeconds(duration);
            
            moveSpeed = originalSpeed;
        }

        public void SetBoundaries(float minX, float maxX, float minY, float maxY)
        {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("EnemyBullet"))
            {
                Bullet bullet = other.GetComponent<Bullet>();
                if (bullet != null)
                {
                    TakeDamage(bullet.Damage);
                    Destroy(other.gameObject);
                }
            }
            else if (other.CompareTag("Enemy"))
            {
                TakeDamage(20);
            }
        }
    }
}
