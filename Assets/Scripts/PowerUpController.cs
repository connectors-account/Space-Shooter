using UnityEngine;

/// <summary>
/// PowerUpController defines a collectible power-up that drifts downward.
/// When the player touches it the effect is applied and the pickup is destroyed.
/// Three types: Health, WeaponUpgrade, Shield.
/// </summary>
public class PowerUpController : MonoBehaviour
{
    // ── Power-Up Types ───────────────────────────────────────
    public enum PowerUpType
    {
        Health,         // Restores 1-2 health points
        WeaponUpgrade,  // Upgrades weapon level
        Shield          // Activates shield for a duration
    }

    [Header("Power-Up Settings")]
    [SerializeField] private PowerUpType type = PowerUpType.Health;
    [SerializeField] private float driftSpeed = 2f;
    [SerializeField] private float lifetime = 8f;
    [SerializeField] private float bobAmplitude = 0.3f;
    [SerializeField] private float bobFrequency = 3f;

    // ── Internal ─────────────────────────────────────────────
    private float spawnTime;
    private float startX;
    private SpriteRenderer spriteRenderer;

    // ── Public Property ──────────────────────────────────────
    public PowerUpType Type => type;

    // ──────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ──────────────────────────────────────────────────────────

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        spawnTime = Time.time;
        startX = transform.position.x;
        ApplyVisualByType();
    }

    private void Update()
    {
        // Drift downward with a gentle horizontal bob
        float xOffset = Mathf.Sin((Time.time - spawnTime) * bobFrequency) * bobAmplitude;
        float newX = startX + xOffset;
        float newY = transform.position.y - driftSpeed * Time.deltaTime;
        transform.position = new Vector3(newX, newY, 0f);

        // Destroy after lifetime
        if (Time.time - spawnTime > lifetime || transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }

    // ──────────────────────────────────────────────────────────
    // Setup
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Set the power-up type. Called by PowerUpSpawner after instantiation.
    /// </summary>
    public void SetType(PowerUpType newType)
    {
        type = newType;
        ApplyVisualByType();
    }

    /// <summary>
    /// Tint the sprite based on type so the player can tell them apart.
    /// </summary>
    private void ApplyVisualByType()
    {
        if (spriteRenderer == null) return;

        switch (type)
        {
            case PowerUpType.Health:
                spriteRenderer.color = new Color(0f, 1f, 0.3f); // green
                break;
            case PowerUpType.WeaponUpgrade:
                spriteRenderer.color = new Color(1f, 0.6f, 0f); // orange
                break;
            case PowerUpType.Shield:
                spriteRenderer.color = new Color(0.3f, 0.6f, 1f); // blue
                break;
        }
    }

    // ──────────────────────────────────────────────────────────
    // Collision – collected by the player
    // ──────────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        // Apply the power-up effect
        switch (type)
        {
            case PowerUpType.Health:
                player.Heal(2);
                break;

            case PowerUpType.WeaponUpgrade:
                player.UpgradeWeapon();
                break;

            case PowerUpType.Shield:
                player.ActivateShield(5f);
                break;
        }

        // Play pickup SFX
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("PowerUp");

        Destroy(gameObject);
    }
}
