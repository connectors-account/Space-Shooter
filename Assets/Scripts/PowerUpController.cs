using UnityEngine;

/// <summary>
/// Controls power-up behavior: type, visual, movement, and effect application.
/// Power-ups drift downward and are collected on player contact.
/// </summary>
public class PowerUpController : MonoBehaviour
{
    public enum PowerUpType
    {
        Health,     // Restores 30 health
        Shield,     // Absorbs one hit
        RapidFire   // Doubles fire rate for 8 seconds
    }

    [Header("Power-Up Settings")]
    [SerializeField] private PowerUpType powerUpType = PowerUpType.Health;
    [SerializeField] private float driftSpeed = 2f;
    [SerializeField] private float bobAmplitude = 0.3f;
    [SerializeField] private float bobFrequency = 3f;
    [SerializeField] private float lifetime = 10f;

    [Header("Effect Values")]
    [SerializeField] private int healAmount = 30;
    [SerializeField] private float rapidFireDuration = 8f;

    private float spawnTime;
    private float baseY;
    private SpriteRenderer spriteRenderer;

    // Colors for different power-up types
    private static readonly Color HealthColor = new Color(0.2f, 1f, 0.2f, 1f);     // Green
    private static readonly Color ShieldColor = new Color(0.2f, 0.6f, 1f, 1f);     // Blue
    private static readonly Color RapidFireColor = new Color(1f, 0.8f, 0.2f, 1f);  // Yellow

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        spawnTime = Time.time;
        baseY = transform.position.y;
        ApplyVisual();
    }

    /// <summary>
    /// Set the type of this power-up.
    /// </summary>
    public void SetType(PowerUpType type)
    {
        powerUpType = type;
        ApplyVisual();
    }

    /// <summary>
    /// Assign a random power-up type.
    /// </summary>
    public void RandomizeType()
    {
        float roll = Random.value;
        if (roll < 0.4f)
            powerUpType = PowerUpType.Health;
        else if (roll < 0.7f)
            powerUpType = PowerUpType.RapidFire;
        else
            powerUpType = PowerUpType.Shield;

        ApplyVisual();
    }

    private void ApplyVisual()
    {
        if (spriteRenderer == null) return;

        switch (powerUpType)
        {
            case PowerUpType.Health:
                spriteRenderer.color = HealthColor;
                break;
            case PowerUpType.Shield:
                spriteRenderer.color = ShieldColor;
                break;
            case PowerUpType.RapidFire:
                spriteRenderer.color = RapidFireColor;
                break;
        }
    }

    private void Update()
    {
        // Drift downward with bobbing
        float elapsed = Time.time - spawnTime;
        float bob = Mathf.Sin(elapsed * bobFrequency) * bobAmplitude;
        transform.position += Vector3.down * driftSpeed * Time.deltaTime;

        Vector3 pos = transform.position;
        pos.x += Mathf.Sin(elapsed * 1.5f) * 0.01f; // Gentle horizontal sway
        transform.position = pos;

        // Rotate slowly for visual appeal
        transform.Rotate(Vector3.forward, 90f * Time.deltaTime);

        // Lifetime check
        if (elapsed > lifetime || transform.position.y < -7f)
        {
            Destroy(gameObject);
        }

        // Blink when about to expire
        if (elapsed > lifetime - 3f && spriteRenderer != null)
        {
            float alpha = Mathf.PingPong(elapsed * 5f, 1f);
            Color c = spriteRenderer.color;
            c.a = 0.3f + alpha * 0.7f;
            spriteRenderer.color = c;
        }
    }

    /// <summary>
    /// Apply this power-up's effect to the player.
    /// Called by CollisionHandler when player collects this power-up.
    /// </summary>
    public void ApplyEffect(PlayerController player)
    {
        if (player == null) return;

        switch (powerUpType)
        {
            case PowerUpType.Health:
                player.HealPlayer(healAmount);
                AudioManager.Instance?.PlaySFX("PowerUpHealth");
                break;

            case PowerUpType.Shield:
                player.ActivateShield();
                AudioManager.Instance?.PlaySFX("PowerUpShield");
                break;

            case PowerUpType.RapidFire:
                player.ActivateRapidFire(rapidFireDuration);
                AudioManager.Instance?.PlaySFX("PowerUpRapidFire");
                break;
        }

        // Show pickup text
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowPowerUpText(powerUpType.ToString());
        }

        Destroy(gameObject);
    }
}
