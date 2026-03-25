// =============================================================================
// PlayerController.cs — Handles player ship movement, shooting, and power-ups
// =============================================================================
using UnityEngine;
using System.Collections;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Main player controller handling movement within screen bounds,
    /// shooting mechanics, power-up effects, and invincibility frames.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float boundaryPaddingX = 0.5f;
        [SerializeField] private float boundaryPaddingY = 0.5f;

        [Header("Shooting")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.2f;
        [SerializeField] private float bulletSpeed = 12f;
        [SerializeField] private bool autoFire = true;

        [Header("Power-Up Durations")]
        [SerializeField] private float rapidFireDuration = 5f;
        [SerializeField] private float spreadShotDuration = 5f;
        [SerializeField] private float shieldDuration = 8f;

        [Header("Visual")]
        [SerializeField] private GameObject shieldVisual;
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private SpriteRenderer spriteRenderer;

        // Internal state
        private float nextFireTime;
        private float screenMinX, screenMaxX, screenMinY, screenMaxY;
        private bool hasShield;
        private bool hasRapidFire;
        private bool hasSpreadShot;
        private bool isInvincible;
        private float invincibilityTimer;
        private float invincibilityDuration = 2f;
        private Coroutine rapidFireCoroutine;
        private Coroutine spreadShotCoroutine;
        private Coroutine shieldCoroutine;

        private HealthSystem healthSystem;
        private Vector2 moveInput;

        /// <summary>
        /// Whether the player currently has an active shield.
        /// </summary>
        public bool HasShield => hasShield;

        /// <summary>
        /// Whether the player is currently invincible (post-hit i-frames).
        /// </summary>
        public bool IsInvincible => isInvincible;

        private void Awake()
        {
            healthSystem = GetComponent<HealthSystem>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            CalculateScreenBounds();
            if (shieldVisual != null)
                shieldVisual.SetActive(false);
        }

        private void Update()
        {
            HandleMovement();
            HandleShooting();
            HandleInvincibility();
        }

        /// <summary>
        /// Calculates the visible screen boundaries in world space.
        /// </summary>
        private void CalculateScreenBounds()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            screenMinX = -camWidth + boundaryPaddingX;
            screenMaxX = camWidth - boundaryPaddingX;
            screenMinY = -camHeight + boundaryPaddingY;
            screenMaxY = camHeight - boundaryPaddingY;
        }

        /// <summary>
        /// Reads input and moves the player, clamped to screen bounds.
        /// </summary>
        private void HandleMovement()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(h, v).normalized;

            Vector3 pos = transform.position;
            pos += (Vector3)(moveInput * moveSpeed * Time.deltaTime);
            pos.x = Mathf.Clamp(pos.x, screenMinX, screenMaxX);
            pos.y = Mathf.Clamp(pos.y, screenMinY, screenMaxY);
            transform.position = pos;
        }

        /// <summary>
        /// Fires bullets based on current fire rate and power-up state.
        /// </summary>
        private void HandleShooting()
        {
            bool wantsToFire = autoFire || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
            if (!wantsToFire) return;
            if (Time.time < nextFireTime) return;

            float currentFireRate = hasRapidFire ? fireRate * 0.4f : fireRate;
            nextFireTime = Time.time + currentFireRate;

            if (hasSpreadShot)
            {
                FireSpread();
            }
            else
            {
                FireSingle();
            }

            Managers.SoundManager.Instance?.PlaySFX("player_shoot");
        }

        /// <summary>
        /// Fires a single bullet straight ahead.
        /// </summary>
        private void FireSingle()
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;
            SpawnBullet(spawnPos, Vector2.up);
        }

        /// <summary>
        /// Fires a spread of 5 bullets in a fan pattern.
        /// </summary>
        private void FireSpread()
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;
            float[] angles = { -20f, -10f, 0f, 10f, 20f };
            foreach (float angle in angles)
            {
                Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;
                SpawnBullet(spawnPos, dir);
            }
        }

        /// <summary>
        /// Instantiates a bullet and sets its velocity.
        /// </summary>
        private void SpawnBullet(Vector3 position, Vector2 direction)
        {
            if (bulletPrefab == null) return;
            GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
            Weapons.Bullet b = bullet.GetComponent<Weapons.Bullet>();
            if (b != null)
            {
                b.Initialize(direction, bulletSpeed, true, 1);
            }
        }

        /// <summary>
        /// Handles post-hit invincibility blinking.
        /// </summary>
        private void HandleInvincibility()
        {
            if (!isInvincible) return;
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                SetAlpha(1f);
                return;
            }
            // Blink effect
            float alpha = Mathf.PingPong(Time.time * 10f, 1f) > 0.5f ? 1f : 0.3f;
            SetAlpha(alpha);
        }

        private void SetAlpha(float a)
        {
            if (spriteRenderer == null) return;
            Color c = spriteRenderer.color;
            c.a = a;
            spriteRenderer.color = c;
        }

        /// <summary>
        /// Called when the player takes damage from an enemy or bullet.
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (isInvincible) return;

            if (hasShield)
            {
                hasShield = false;
                if (shieldVisual != null) shieldVisual.SetActive(false);
                ActivateInvincibility();
                Managers.SoundManager.Instance?.PlaySFX("shield_break");
                return;
            }

            if (healthSystem != null)
            {
                healthSystem.TakeDamage(damage);
                Managers.SoundManager.Instance?.PlaySFX("player_hit");

                if (healthSystem.CurrentHealth <= 0)
                {
                    Die();
                }
                else
                {
                    ActivateInvincibility();
                }
            }
        }

        /// <summary>
        /// Activates post-hit invincibility frames.
        /// </summary>
        private void ActivateInvincibility()
        {
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
        }

        /// <summary>
        /// Handles player death: explosion, lives, game over.
        /// </summary>
        private void Die()
        {
            Managers.SoundManager.Instance?.PlaySFX("player_explode");
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            Managers.GameManager gm = Managers.GameManager.Instance;
            if (gm != null)
            {
                gm.PlayerDied();
            }
        }

        /// <summary>
        /// Respawn the player at the starting position.
        /// </summary>
        public void Respawn(Vector3 position)
        {
            transform.position = position;
            if (healthSystem != null) healthSystem.ResetHealth();
            hasShield = false;
            hasRapidFire = false;
            hasSpreadShot = false;
            if (shieldVisual != null) shieldVisual.SetActive(false);
            ActivateInvincibility();
            gameObject.SetActive(true);
        }

        // ============================
        // Power-Up Activation Methods
        // ============================

        public void ActivateShield()
        {
            if (shieldCoroutine != null) StopCoroutine(shieldCoroutine);
            shieldCoroutine = StartCoroutine(ShieldRoutine());
        }

        public void ActivateRapidFire()
        {
            if (rapidFireCoroutine != null) StopCoroutine(rapidFireCoroutine);
            rapidFireCoroutine = StartCoroutine(RapidFireRoutine());
        }

        public void ActivateSpreadShot()
        {
            if (spreadShotCoroutine != null) StopCoroutine(spreadShotCoroutine);
            spreadShotCoroutine = StartCoroutine(SpreadShotRoutine());
        }

        public void HealPlayer(int amount)
        {
            if (healthSystem != null)
            {
                healthSystem.Heal(amount);
            }
        }

        public void AddLife()
        {
            Managers.GameManager gm = Managers.GameManager.Instance;
            if (gm != null) gm.AddLife();
        }

        private IEnumerator ShieldRoutine()
        {
            hasShield = true;
            if (shieldVisual != null) shieldVisual.SetActive(true);
            yield return new WaitForSeconds(shieldDuration);
            hasShield = false;
            if (shieldVisual != null) shieldVisual.SetActive(false);
        }

        private IEnumerator RapidFireRoutine()
        {
            hasRapidFire = true;
            yield return new WaitForSeconds(rapidFireDuration);
            hasRapidFire = false;
        }

        private IEnumerator SpreadShotRoutine()
        {
            hasSpreadShot = true;
            yield return new WaitForSeconds(spreadShotDuration);
            hasSpreadShot = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Handle collision with enemy bullets
            if (other.CompareTag("EnemyBullet"))
            {
                Weapons.Bullet bullet = other.GetComponent<Weapons.Bullet>();
                int dmg = bullet != null ? bullet.Damage : 1;
                TakeDamage(dmg);
                Destroy(other.gameObject);
            }
            // Handle collision with enemy ships
            else if (other.CompareTag("Enemy"))
            {
                TakeDamage(1);
            }
        }
    }
}
