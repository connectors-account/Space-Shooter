using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float padding = 0.5f;

    [Header("Combat")]
    [SerializeField] private GameObject playerBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float baseFireRate = 0.25f;
    [SerializeField] private int maxHealth = 100;

    [Header("Power-Up Settings")]
    [SerializeField] private float rapidFireMultiplier = 0.4f;

    private int currentHealth;
    private float nextShotTime;
    private bool shieldActive;
    private float shieldEndTime;
    private float rapidFireEndTime;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool ShieldActive => shieldActive;

    private void Start()
    {
        currentHealth = maxHealth;
        UIManager.Instance?.UpdateHealth(currentHealth, maxHealth);
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        HandleMovement();
        HandleShooting();
        UpdateTimedEffects();
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector3 movement = new Vector3(moveX, moveY, 0f).normalized * (moveSpeed * Time.deltaTime);
        transform.position += movement;

        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 min = cam.ViewportToWorldPoint(new Vector3(0f, 0f, cam.nearClipPlane));
            Vector3 max = cam.ViewportToWorldPoint(new Vector3(1f, 1f, cam.nearClipPlane));
            float clampedX = Mathf.Clamp(transform.position.x, min.x + padding, max.x - padding);
            float clampedY = Mathf.Clamp(transform.position.y, min.y + padding, max.y - padding);
            transform.position = new Vector3(clampedX, clampedY, 0f);
        }
    }

    private void HandleShooting()
    {
        if (!Input.GetKey(KeyCode.Space))
        {
            return;
        }

        if (Time.time < nextShotTime)
        {
            return;
        }

        if (playerBulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Player bullet prefab or fire point is not assigned.");
            return;
        }

        GameObject bullet = Instantiate(playerBulletPrefab, firePoint.position, Quaternion.identity);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.Initialize(Vector2.up, false, 1);
        }

        AudioManager.Instance?.PlayPlayerShoot(); // Add shoot SFX clip in AudioManager inspector

        float fireRate = GetCurrentFireRate();
        nextShotTime = Time.time + fireRate;
    }

    private float GetCurrentFireRate()
    {
        bool rapid = Time.time < rapidFireEndTime;
        return rapid ? baseFireRate * rapidFireMultiplier : baseFireRate;
    }

    private void UpdateTimedEffects()
    {
        if (shieldActive && Time.time >= shieldEndTime)
        {
            shieldActive = false;
            UIManager.Instance?.SetShieldIndicator(false);
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || shieldActive)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        UIManager.Instance?.UpdateHealth(currentHealth, maxHealth);
        AudioManager.Instance?.PlayPlayerHit(); // Add player hit SFX clip in AudioManager inspector

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UIManager.Instance?.UpdateHealth(currentHealth, maxHealth);
    }

    public void ActivateShield(float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        shieldActive = true;
        shieldEndTime = Time.time + duration;
        UIManager.Instance?.SetShieldIndicator(true);
    }

    public void ActivateRapidFire(float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        rapidFireEndTime = Time.time + duration;
        UIManager.Instance?.SetRapidFireIndicator(true, duration);
    }

    private void Die()
    {
        AudioManager.Instance?.PlayPlayerDeath(); // Add player death SFX clip in AudioManager inspector
        GameManager.Instance?.GameOver();
        Destroy(gameObject);
    }
}
