using UnityEngine;
using System.Collections;

/// <summary>
/// Player ship controller: handles movement, shooting, power-ups, and damage.
/// </summary>
[RequireComponent(typeof(HealthManager))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float screenBorderX = 8.5f;
    public float screenBorderY = 4.5f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;

    [Header("Multi-Shot")]
    public GameObject tripleShotBulletPrefab;

    [Header("Shield")]
    public GameObject shieldVisual;

    [Header("Invulnerability")]
    public float invulnerabilityDuration = 2f;
    public float blinkInterval = 0.15f;

    [Header("Engine Effects")]
    public GameObject leftEngine;
    public GameObject rightEngine;

    // Runtime state
    private HealthManager healthManager;
    private SpriteRenderer spriteRenderer;
    private float nextFireTime;
    private bool hasMultiShot;
    private bool hasShield;
    private bool hasSpeedBoost;
    private float multiShotTimer;
    private float speedBoostTimer;
    private float currentSpeedMultiplier = 1f;
    private bool isRespawning;

    public bool HasShield => hasShield;
    public HealthManager Health => healthManager;

    private void Awake()
    {
        Instance = this;
        healthManager = GetComponent<HealthManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        healthManager.OnDeath += OnPlayerDeath;
        healthManager.OnHealthChanged += OnHealthChanged;

        if (shieldVisual != null)
            shieldVisual.SetActive(false);

        UIManager.Instance?.UpdateHealth(healthManager.HealthPercent);
        UIManager.Instance?.UpdateLives(GameManager.Instance.Lives);
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (isRespawning) return;

        HandleMovement();
        HandleShooting();
        UpdatePowerUpTimers();
    }

    private void HandleMovement()
    {
        float hInput = Input.GetAxisRaw("Horizontal");
        float vInput = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(hInput, vInput, 0f).normalized;
        float speed = moveSpeed * currentSpeedMultiplier;
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Clamp position to screen bounds
        float clampedX = Mathf.Clamp(transform.position.x, -screenBorderX, screenBorderX);
        float clampedY = Mathf.Clamp(transform.position.y, -screenBorderY, screenBorderY);
        transform.position = new Vector3(clampedX, clampedY, 0f);
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
        if (firePoint == null) return;

        AudioManager.Instance?.PlaySFX("PlayerShoot");

        if (hasMultiShot && tripleShotBulletPrefab != null)
        {
            // Triple shot: center + two angled
            Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Instantiate(tripleShotBulletPrefab, firePoint.position + Vector3.left * 0.3f,
                Quaternion.Euler(0, 0, 10f));
            Instantiate(tripleShotBulletPrefab, firePoint.position + Vector3.right * 0.3f,
                Quaternion.Euler(0, 0, -10f));
        }
        else
        {
            Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        }
    }

    private void UpdatePowerUpTimers()
    {
        if (hasMultiShot)
        {
            multiShotTimer -= Time.deltaTime;
            if (multiShotTimer <= 0f)
            {
                hasMultiShot = false;
            }
        }

        if (hasSpeedBoost)
        {
            speedBoostTimer -= Time.deltaTime;
            if (speedBoostTimer <= 0f)
            {
                hasSpeedBoost = false;
                currentSpeedMultiplier = 1f;
            }
        }
    }

    // --- Power-Up Activation ---

    public void ActivateShield()
    {
        hasShield = true;
        if (shieldVisual != null)
            shieldVisual.SetActive(true);
        AudioManager.Instance?.PlaySFX("PowerUp");
    }

    public void DeactivateShield()
    {
        hasShield = false;
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }

    public void ActivateMultiShot(float duration = 5f)
    {
        hasMultiShot = true;
        multiShotTimer = duration;
        AudioManager.Instance?.PlaySFX("PowerUp");
    }

    public void ActivateSpeedBoost(float duration = 5f, float multiplier = 1.5f)
    {
        hasSpeedBoost = true;
        speedBoostTimer = duration;
        currentSpeedMultiplier = multiplier;
        AudioManager.Instance?.PlaySFX("PowerUp");
    }

    // --- Damage Handling ---

    /// <summary>Called when a projectile or enemy hits the player.</summary>
    public void HandleHit(int damage)
    {
        if (isRespawning) return;

        if (hasShield)
        {
            DeactivateShield();
            AudioManager.Instance?.PlaySFX("ShieldHit");
            return;
        }

        healthManager.TakeDamage(damage);
        AudioManager.Instance?.PlaySFX("PlayerHit");
    }

    private void OnHealthChanged(int current, int max)
    {
        float pct = (float)current / max;
        UIManager.Instance?.UpdateHealth(pct);

        // Show engine damage at low health
        if (leftEngine != null)
            leftEngine.SetActive(pct < 0.6f);
        if (rightEngine != null)
            rightEngine.SetActive(pct < 0.3f);
    }

    private void OnPlayerDeath()
    {
        AudioManager.Instance?.PlaySFX("PlayerExplosion");
        ScoreManager.Instance?.ResetCombo();

        bool gameOver = GameManager.Instance.LoseLife();
        if (!gameOver)
        {
            StartCoroutine(RespawnRoutine());
        }
        else
        {
            gameObject.SetActive(false);
            SpawnManager.Instance?.StopSpawning();
        }
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;
        spriteRenderer.enabled = false;

        yield return new WaitForSeconds(GameManager.Instance.respawnDelay);

        // Reset position and health
        transform.position = new Vector3(0f, -3.5f, 0f);
        healthManager.ResetHealth();
        spriteRenderer.enabled = true;

        // Brief invulnerability
        healthManager.isInvulnerable = true;
        float elapsed = 0f;
        while (elapsed < invulnerabilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }
        spriteRenderer.enabled = true;
        healthManager.isInvulnerable = false;
        isRespawning = false;
    }

    private void OnDestroy()
    {
        if (healthManager != null)
        {
            healthManager.OnDeath -= OnPlayerDeath;
            healthManager.OnHealthChanged -= OnHealthChanged;
        }
    }
}
