using UnityEngine;

/// <summary>
/// PowerUp represents a collectible power-up that provides bonuses to the player.
/// </summary>
public class PowerUp : MonoBehaviour, IPooledObject
{
    [Header("Power-Up Settings")]
    [SerializeField] private PowerUpType type = PowerUpType.WeaponUpgrade;
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private float bobAmount = 0.2f;
    [SerializeField] private float bobSpeed = 3f;

    [Header("Effect Values")]
    [SerializeField] private int healthAmount = 1;
    [SerializeField] private float shieldDuration = 5f;
    [SerializeField] private int scoreBonus = 500;

    [Header("Audio")]
    [SerializeField] private string collectSoundName = "PowerUp";

    // Private variables
    private float lifeTimer;
    private float bobTimer;
    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;

    public enum PowerUpType
    {
        WeaponUpgrade,
        Shield,
        Health,
        ScoreBonus,
        SpeedBoost
    }

    public PowerUpType Type => type;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnObjectSpawn()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
    }

    /// <summary>
    /// Initialize power-up state
    /// </summary>
    private void Initialize()
    {
        lifeTimer = lifetime;
        bobTimer = 0f;
        startPosition = transform.position;

        // Set color based on type
        SetColorByType();
    }

    /// <summary>
    /// Set sprite color based on power-up type
    /// </summary>
    private void SetColorByType()
    {
        if (spriteRenderer == null) return;

        switch (type)
        {
            case PowerUpType.WeaponUpgrade:
                spriteRenderer.color = Color.yellow;
                break;
            case PowerUpType.Shield:
                spriteRenderer.color = Color.cyan;
                break;
            case PowerUpType.Health:
                spriteRenderer.color = Color.green;
                break;
            case PowerUpType.ScoreBonus:
                spriteRenderer.color = new Color(1f, 0.5f, 0f); // Orange
                break;
            case PowerUpType.SpeedBoost:
                spriteRenderer.color = Color.magenta;
                break;
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        Move();
        UpdateLifetime();
    }

    /// <summary>
    /// Move the power-up downward with bobbing effect
    /// </summary>
    private void Move()
    {
        bobTimer += Time.deltaTime * bobSpeed;
        float bobOffset = Mathf.Sin(bobTimer) * bobAmount;

        // Move down
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        // Apply horizontal bob
        Vector3 pos = transform.position;
        pos.x = startPosition.x + bobOffset;
        transform.position = pos;

        // Rotate for visual effect
        transform.Rotate(0, 0, 90f * Time.deltaTime);
    }

    /// <summary>
    /// Update lifetime and flash when about to expire
    /// </summary>
    private void UpdateLifetime()
    {
        lifeTimer -= Time.deltaTime;

        // Flash when about to expire
        if (lifeTimer <= 2f && spriteRenderer != null)
        {
            float flash = Mathf.PingPong(Time.time * 8f, 1f);
            Color color = spriteRenderer.color;
            color.a = 0.5f + flash * 0.5f;
            spriteRenderer.color = color;
        }

        // Deactivate when expired
        if (lifeTimer <= 0f || transform.position.y < -6f)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Apply power-up effect to player
    /// </summary>
    private void ApplyEffect(GameObject player)
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        switch (type)
        {
            case PowerUpType.WeaponUpgrade:
                if (playerController != null)
                {
                    playerController.UpgradeWeapon();
                }
                break;

            case PowerUpType.Shield:
                if (playerController != null)
                {
                    playerController.ActivateShield(shieldDuration);
                }
                break;

            case PowerUpType.Health:
                if (playerHealth != null)
                {
                    playerHealth.Heal(healthAmount);
                }
                break;

            case PowerUpType.ScoreBonus:
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(scoreBonus);
                }
                break;

            case PowerUpType.SpeedBoost:
                // Could add a speed boost mechanic
                Debug.Log("Speed boost collected!");
                break;
        }
    }

    /// <summary>
    /// Handle collision with player
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Play collect sound
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound(collectSoundName);
            }

            // Apply effect
            ApplyEffect(other.gameObject);

            // Deactivate
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Set power-up type (used by spawner)
    /// </summary>
    public void SetType(PowerUpType newType)
    {
        type = newType;
        SetColorByType();
    }
}
