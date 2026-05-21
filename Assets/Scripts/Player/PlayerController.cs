using UnityEngine;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Handles player ship movement, shooting, health, and power-up state.
    /// Attach to the Player ship GameObject.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float boundaryPadding = 0.5f;

        [Header("Shooting")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.25f;
        [SerializeField] private float rapidFireRate = 0.1f;
        [SerializeField] private string bulletPoolTag = "PlayerBullet";

        [Header("Health")]
        [SerializeField] private int maxLives = 5;
        [SerializeField] private float invincibilityDuration = 2f;

        [Header("Shield")]
        [SerializeField] private GameObject shieldVisual;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float flashInterval = 0.15f;

        // State
        private Rigidbody2D rb;
        private int currentLives;
        private float nextFireTime;
        private bool isInvincible;
        private float invincibilityTimer;
        private bool isShieldActive;
        private float shieldTimer;
        private bool isRapidFire;
        private float rapidFireTimer;
        private Vector2 screenBounds;
        private float flashTimer;
        private bool isVisible = true;

        // Properties
        public int CurrentLives => currentLives;
        public int MaxLives => maxLives;
        public bool IsShieldActive => isShieldActive;
        public bool IsRapidFire => isRapidFire;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void Start()
        {
            currentLives = maxLives;

            // Calculate screen bounds in world coordinates
            Camera cam = Camera.main;
            if (cam != null)
            {
                screenBounds = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.transform.position.z));
            }
            else
            {
                screenBounds = new Vector2(8.9f, 5f);
            }

            if (shieldVisual != null)
                shieldVisual.SetActive(false);

            if (firePoint == null)
            {
                GameObject fp = new GameObject("FirePoint");
                fp.transform.SetParent(transform);
                fp.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                firePoint = fp.transform;
            }
        }

        private void Update()
        {
            HandleShooting();
            HandlePowerUpTimers();
            HandleInvincibilityVisual();
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector2 movement = new Vector2(h, v).normalized * moveSpeed;
            rb.linearVelocity = movement;

            // Clamp position to screen bounds
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, -Mathf.Abs(screenBounds.x) + boundaryPadding, Mathf.Abs(screenBounds.x) - boundaryPadding);
            pos.y = Mathf.Clamp(pos.y, -Mathf.Abs(screenBounds.y) + boundaryPadding, Mathf.Abs(screenBounds.y) - boundaryPadding);
            transform.position = pos;
        }

        private void HandleShooting()
        {
            if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
            {
                float currentFireRate = isRapidFire ? rapidFireRate : fireRate;
                nextFireTime = Time.time + currentFireRate;
                FireBullet();
            }
        }

        private void FireBullet()
        {
            GameObject bullet = Managers.ObjectPoolManager.Instance.GetFromPool(
                bulletPoolTag, firePoint.position, Quaternion.identity);

            if (bullet != null)
            {
                Weapons.Bullet bulletComp = bullet.GetComponent<Weapons.Bullet>();
                if (bulletComp != null)
                {
                    bulletComp.Initialize(Vector2.up, true);
                }
            }

            Managers.AudioManager.Instance?.PlaySFX("PlayerShoot");
        }

        private void HandlePowerUpTimers()
        {
            if (isRapidFire)
            {
                rapidFireTimer -= Time.deltaTime;
                if (rapidFireTimer <= 0f)
                {
                    isRapidFire = false;
                }
            }

            if (isShieldActive)
            {
                shieldTimer -= Time.deltaTime;
                if (shieldTimer <= 0f)
                {
                    DeactivateShield();
                }
            }

            if (isInvincible)
            {
                invincibilityTimer -= Time.deltaTime;
                if (invincibilityTimer <= 0f)
                {
                    isInvincible = false;
                    if (spriteRenderer != null)
                    {
                        Color c = spriteRenderer.color;
                        c.a = 1f;
                        spriteRenderer.color = c;
                    }
                    isVisible = true;
                }
            }
        }

        private void HandleInvincibilityVisual()
        {
            if (isInvincible && spriteRenderer != null)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0f)
                {
                    isVisible = !isVisible;
                    Color c = spriteRenderer.color;
                    c.a = isVisible ? 1f : 0.3f;
                    spriteRenderer.color = c;
                    flashTimer = flashInterval;
                }
            }
        }

        public void TakeDamage(int damage = 1)
        {
            if (isInvincible) return;

            if (isShieldActive)
            {
                DeactivateShield();
                StartInvincibility();
                Managers.AudioManager.Instance?.PlaySFX("ShieldBreak");
                return;
            }

            currentLives -= damage;
            Managers.AudioManager.Instance?.PlaySFX("PlayerHit");

            Managers.GameManager.Instance?.OnPlayerHealthChanged(currentLives, maxLives);

            if (currentLives <= 0)
            {
                Die();
            }
            else
            {
                StartInvincibility();
            }
        }

        private void StartInvincibility()
        {
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
            flashTimer = flashInterval;
        }

        private void Die()
        {
            // Spawn explosion effect
            Effects.ExplosionManager.Instance?.SpawnExplosion(transform.position, Effects.ExplosionType.Large);
            Managers.AudioManager.Instance?.PlaySFX("PlayerDeath");
            Managers.GameManager.Instance?.OnPlayerDeath();
            gameObject.SetActive(false);
        }

        // Power-up activation methods
        public void ActivateRapidFire(float duration)
        {
            isRapidFire = true;
            rapidFireTimer = duration;
            Managers.AudioManager.Instance?.PlaySFX("PowerUp");
        }

        public void ActivateShield(float duration)
        {
            isShieldActive = true;
            shieldTimer = duration;
            if (shieldVisual != null)
                shieldVisual.SetActive(true);
            Managers.AudioManager.Instance?.PlaySFX("PowerUp");
        }

        private void DeactivateShield()
        {
            isShieldActive = false;
            if (shieldVisual != null)
                shieldVisual.SetActive(false);
        }

        public void RestoreHealth(int amount)
        {
            currentLives = Mathf.Min(currentLives + amount, maxLives);
            Managers.GameManager.Instance?.OnPlayerHealthChanged(currentLives, maxLives);
            Managers.AudioManager.Instance?.PlaySFX("PowerUp");
        }

        public void ResetPlayer()
        {
            currentLives = maxLives;
            isShieldActive = false;
            isRapidFire = false;
            isInvincible = false;
            if (shieldVisual != null)
                shieldVisual.SetActive(false);
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }
            transform.position = new Vector3(0f, -3.5f, 0f);
            gameObject.SetActive(true);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("EnemyBullet"))
            {
                TakeDamage(1);
                other.gameObject.SetActive(false);
            }
            else if (other.CompareTag("Enemy"))
            {
                TakeDamage(1);
            }
            else if (other.CompareTag("PowerUp"))
            {
                PowerUps.PowerUpItem pu = other.GetComponent<PowerUps.PowerUpItem>();
                if (pu != null)
                {
                    pu.ApplyEffect(this);
                }
            }
        }
    }
}
