using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float smoothTime = 0.1f;

    [Header("Boundaries")]
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4f;
    public float maxY = 4f;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;
    public float rapidFireRate = 0.1f;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip hitSound;
    public AudioClip deathSound;
    public AudioClip powerUpSound;

    // State
    private Vector2 velocity;
    private float nextFireTime;
    private bool canShoot = true;
    private bool hasRapidFire = false;
    private float rapidFireEndTime;
    private bool isInvincible = false;
    private float invincibilityEndTime;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private HealthSystem healthSystem;
    private AudioSource audioSource;

    // Events
    public static event Action OnPlayerDeath;
    public static event Action<int> OnLivesChanged;

    private int currentLives;
    public int CurrentLives => currentLives;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        healthSystem = GetComponent<HealthSystem>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        currentLives = GameManager.Instance != null ? GameManager.Instance.startingLives : 3;
        OnLivesChanged?.Invoke(currentLives);

        if (healthSystem != null)
        {
            healthSystem.OnDeath += HandleDeath;
            healthSystem.OnDamageTaken += HandleDamageTaken;
        }
    }

    private void Update()
    {
        if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing)
            return;

        HandleMovement();
        HandleShooting();
        HandlePowerUpTimers();
        HandleInvincibility();
    }

    private void HandleMovement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector2 targetVelocity = new Vector2(horizontalInput, verticalInput).normalized * moveSpeed;
        rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref velocity, smoothTime);

        // Clamp position to boundaries
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        transform.position = clampedPosition;
    }

    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && canShoot && Time.time >= nextFireTime)
        {
            Shoot();
            float currentFireRate = hasRapidFire ? rapidFireRate : fireRate;
            nextFireTime = Time.time + currentFireRate;
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        // Try to use object pooler first
        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.SpawnFromPool("PlayerBullet", firePoint.position, Quaternion.identity);
        }
        else
        {
            Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        }

        // Play shoot sound
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        AudioManager.Instance?.PlaySFX("PlayerShoot");
    }

    private void HandlePowerUpTimers()
    {
        if (hasRapidFire && Time.time >= rapidFireEndTime)
        {
            hasRapidFire = false;
        }
    }

    private void HandleInvincibility()
    {
        if (isInvincible)
        {
            // Blink effect
            float alpha = Mathf.PingPong(Time.time * 10f, 1f);
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 0.3f + alpha * 0.7f;
                spriteRenderer.color = color;
            }

            if (Time.time >= invincibilityEndTime)
            {
                isInvincible = false;
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = 1f;
                    spriteRenderer.color = color;
                }
            }
        }
    }

    private void HandleDamageTaken(int damage)
    {
        if (isInvincible)
            return;

        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        AudioManager.Instance?.PlaySFX("PlayerHit");
    }

    private void HandleDeath()
    {
        currentLives--;
        OnLivesChanged?.Invoke(currentLives);

        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        AudioManager.Instance?.PlaySFX("PlayerDeath");

        if (currentLives <= 0)
        {
            OnPlayerDeath?.Invoke();
            GameManager.Instance?.GameOver();
            gameObject.SetActive(false);
        }
        else
        {
            // Respawn with invincibility
            healthSystem?.ResetHealth();
            StartInvincibility(GameManager.Instance?.invincibilityDuration ?? 2f);
            transform.position = new Vector3(0, minY + 1f, 0);
        }
    }

    public void StartInvincibility(float duration)
    {
        isInvincible = true;
        invincibilityEndTime = Time.time + duration;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    public void ActivateRapidFire(float duration)
    {
        hasRapidFire = true;
        rapidFireEndTime = Time.time + duration;

        if (powerUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(powerUpSound);
        }
        AudioManager.Instance?.PlaySFX("PowerUp");
    }

    public void ActivateShield(float duration)
    {
        StartInvincibility(duration);

        if (powerUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(powerUpSound);
        }
        AudioManager.Instance?.PlaySFX("PowerUp");
    }

    public void RestoreHealth(int amount)
    {
        healthSystem?.Heal(amount);

        if (powerUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(powerUpSound);
        }
        AudioManager.Instance?.PlaySFX("PowerUp");
    }

    public void AddLife()
    {
        currentLives++;
        OnLivesChanged?.Invoke(currentLives);

        if (powerUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(powerUpSound);
        }
        AudioManager.Instance?.PlaySFX("PowerUp");
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= HandleDeath;
            healthSystem.OnDamageTaken -= HandleDamageTaken;
        }
    }
}
