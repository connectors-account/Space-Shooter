using UnityEngine;

/// <summary>
/// Power-up item that floats downward and applies an effect on player contact.
/// Attach to each power-up prefab variant.
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { HealthRestore, SpreadShot, Shield, RapidFire }

    [Header("Config")]
    public PowerUpType type = PowerUpType.HealthRestore;
    public float fallSpeed = 2f;
    public float effectDuration = 10f;  // seconds for timed effects

    [Header("Visual Indicator")]
    public Color glowColor = Color.green;
    public float pulseSpeed = 4f;

    private SpriteRenderer sr;
    private Color baseColor;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;

        // Tint the sprite to indicate type
        switch (type)
        {
            case PowerUpType.HealthRestore: glowColor = Color.green;  break;
            case PowerUpType.SpreadShot:    glowColor = Color.yellow; break;
            case PowerUpType.Shield:        glowColor = Color.cyan;   break;
            case PowerUpType.RapidFire:     glowColor = Color.red;    break;
        }
        sr.color = glowColor;
    }

    private void Update()
    {
        // Float downward
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        // Pulsing glow
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        sr.color = Color.Lerp(glowColor * 0.6f, glowColor, t);

        // Destroy if off-screen
        if (ScreenBounds.Instance != null && ScreenBounds.Instance.IsOffScreen(transform.position))
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.ApplyPowerUp(type, effectDuration);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayPowerup();
        }

        Destroy(gameObject);
    }
}
