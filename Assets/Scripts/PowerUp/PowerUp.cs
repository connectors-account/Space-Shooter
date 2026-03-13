using UnityEngine;

/// <summary>
/// Power-up item that falls from destroyed enemies.
/// Different types: WeaponUpgrade, Shield, Health.
/// Collected on contact with player.
/// </summary>
public class PowerUp : MonoBehaviour
{
    public enum PowerUpType
    {
        WeaponUpgrade,
        Shield,
        Health
    }

    [Header("Settings")]
    public PowerUpType type = PowerUpType.WeaponUpgrade;
    public float fallSpeed = 2f;
    public float lifetime = 10f;
    public int healAmount = 30;

    [Header("Visual")]
    public float bobAmplitude = 0.2f;
    public float bobFrequency = 3f;

    private float lifeTimer;
    private float startY;
    private float bobOffset;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        lifeTimer = lifetime;
        startY = transform.position.y;
        bobOffset = Random.Range(0f, Mathf.PI * 2f);
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Set color based on type
        if (spriteRenderer != null)
        {
            switch (type)
            {
                case PowerUpType.WeaponUpgrade:
                    spriteRenderer.color = new Color(1f, 0.5f, 0f); // Orange
                    break;
                case PowerUpType.Shield:
                    spriteRenderer.color = new Color(0.3f, 0.5f, 1f); // Blue
                    break;
                case PowerUpType.Health:
                    spriteRenderer.color = new Color(0.2f, 1f, 0.2f); // Green
                    break;
            }
        }
    }

    private void Update()
    {
        // Fall downward with bobbing effect
        float bob = Mathf.Sin((Time.time + bobOffset) * bobFrequency) * bobAmplitude;
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
        // Apply bob as local offset on X
        transform.localPosition = new Vector3(
            transform.localPosition.x + Mathf.Sin((Time.time + bobOffset) * bobFrequency) * 0.01f,
            transform.localPosition.y,
            0f
        );

        // Rotate slightly for visual flair
        transform.Rotate(0, 0, 90f * Time.deltaTime);

        // Lifetime countdown with blink warning
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 3f && spriteRenderer != null)
        {
            float alpha = Mathf.PingPong(Time.time * 5f, 1f) * 0.5f + 0.5f;
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }

        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        // Destroy if off screen
        if (GameBounds.Instance != null && GameBounds.Instance.IsOutOfBounds(transform.position))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            ApplyPowerUp(player);
            AudioManager.Instance?.PlaySFX("PowerUp");
            Destroy(gameObject);
        }
    }

    private void ApplyPowerUp(PlayerController player)
    {
        switch (type)
        {
            case PowerUpType.WeaponUpgrade:
                player.UpgradeWeapon();
                break;
            case PowerUpType.Shield:
                player.ActivateShield();
                break;
            case PowerUpType.Health:
                player.Heal(healAmount);
                break;
        }
    }
}
