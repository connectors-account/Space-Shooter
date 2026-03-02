using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float horizontalBoundary = 8f;
    public float verticalBoundary = 4.5f;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;
    public int weaponLevel = 1;
    public int maxWeaponLevel = 3;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip hitSound;
    public AudioClip deathSound;
    public AudioClip powerUpSound;

    private float nextFireTime = 0f;
    private HealthSystem healthSystem;
    private AudioSource audioSource;
    private bool isShieldActive = false;
    private float shieldDuration = 0f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused())
            return;

        HandleMovement();
        HandleShooting();
        UpdateShield();
    }

    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0f) * moveSpeed * Time.deltaTime;
        transform.position += movement;

        // Clamp position within boundaries
        float clampedX = Mathf.Clamp(transform.position.x, -horizontalBoundary, horizontalBoundary);
        float clampedY = Mathf.Clamp(transform.position.y, -verticalBoundary, verticalBoundary);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Fire()
    {
        if (bulletPrefab == null || firePoint == null) return;

        PlaySound(shootSound);

        switch (weaponLevel)
        {
            case 1:
                SpawnBullet(firePoint.position, Vector2.up);
                break;
            case 2:
                SpawnBullet(firePoint.position + new Vector3(-0.2f, 0, 0), Vector2.up);
                SpawnBullet(firePoint.position + new Vector3(0.2f, 0, 0), Vector2.up);
                break;
            case 3:
                SpawnBullet(firePoint.position, Vector2.up);
                SpawnBullet(firePoint.position + new Vector3(-0.3f, 0, 0), new Vector2(-0.1f, 1f).normalized);
                SpawnBullet(firePoint.position + new Vector3(0.3f, 0, 0), new Vector2(0.1f, 1f).normalized);
                break;
        }
    }

    void SpawnBullet(Vector3 position, Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(direction);
            bulletScript.isPlayerBullet = true;
        }
    }

    void UpdateShield()
    {
        if (isShieldActive)
        {
            shieldDuration -= Time.deltaTime;
            if (shieldDuration <= 0)
            {
                DeactivateShield();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isShieldActive) return;

        PlaySound(hitSound);
        
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(damage);
            
            if (healthSystem.GetCurrentHealth() <= 0)
            {
                Die();
            }
        }
    }

    void Die()
    {
        PlaySound(deathSound);
        GameManager.Instance?.GameOver();
        gameObject.SetActive(false);
    }

    public void UpgradeWeapon()
    {
        PlaySound(powerUpSound);
        if (weaponLevel < maxWeaponLevel)
        {
            weaponLevel++;
        }
    }

    public void Heal(int amount)
    {
        PlaySound(powerUpSound);
        if (healthSystem != null)
        {
            healthSystem.Heal(amount);
        }
    }

    public void ActivateShield(float duration)
    {
        PlaySound(powerUpSound);
        isShieldActive = true;
        shieldDuration = duration;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.5f, 0.5f, 1f, 1f); // Blue tint for shield
        }
    }

    void DeactivateShield()
    {
        isShieldActive = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
        }
        else if (other.CompareTag("EnemyBullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }
}
