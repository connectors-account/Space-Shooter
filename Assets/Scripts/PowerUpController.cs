using UnityEngine;

/// <summary>
/// Power-up types available in the game.
/// </summary>
public enum PowerUpType
{
    Shield,     // Absorbs one hit
    RapidFire,  // Increases fire rate temporarily
    MultiShot,  // Fires three bullets temporarily
    Health      // Restores one health point
}

/// <summary>
/// Controls power-up behavior: drifts downward, bobs, and applies effect on pickup.
/// </summary>
public class PowerUpController : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float bobAmplitude = 0.3f;
    [SerializeField] private float bobFrequency = 3f;
    [SerializeField] private float powerUpDuration = 8f;
    [SerializeField] private float lifetime = 10f;

    private PowerUpType type;
    private float spawnTime;
    private float startY;
    private SpriteRenderer sr;

    // Color mapping for each power-up type
    private static readonly Color[] typeColors = new Color[]
    {
        new Color(0.3f, 0.7f, 1f),   // Shield — cyan
        new Color(1f, 0.8f, 0f),     // RapidFire — gold
        new Color(0.8f, 0.3f, 1f),   // MultiShot — purple
        new Color(0.3f, 1f, 0.3f)    // Health — green
    };

    private void Start()
    {
        spawnTime = Time.time;
        startY = transform.position.y;
        sr = GetComponent<SpriteRenderer>();

        // Randomize power-up type
        type = (PowerUpType)Random.Range(0, System.Enum.GetValues(typeof(PowerUpType)).Length);

        // Set color based on type
        if (sr != null)
        {
            sr.color = typeColors[(int)type];
        }
    }

    private void Update()
    {
        // Drift downward with a gentle bob
        float elapsed = Time.time - spawnTime;
        float bobOffset = Mathf.Sin(elapsed * bobFrequency) * bobAmplitude;
        Vector3 pos = transform.position;
        pos.y -= fallSpeed * Time.deltaTime;
        // Apply horizontal bob
        pos.x += Mathf.Cos(elapsed * bobFrequency) * bobAmplitude * Time.deltaTime;
        transform.position = pos;

        // Gentle rotation for visual appeal
        transform.Rotate(0, 0, 90f * Time.deltaTime);

        // Flash before expiring
        if (elapsed > lifetime - 2f && sr != null)
        {
            float alpha = Mathf.PingPong(Time.time * 5f, 1f) > 0.5f ? 1f : 0.3f;
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        // Destroy after lifetime
        if (elapsed > lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.ApplyPowerUp(type, powerUpDuration);
            AudioManager.Instance?.PlaySFX("PowerUp");

            // Show brief pickup text
            if (GameManager.Instance != null)
            {
                string label = GetPowerUpLabel();
                GameManager.Instance.ShowPowerUpText(label);
            }

            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Returns a human-readable label for the power-up type.
    /// </summary>
    private string GetPowerUpLabel()
    {
        switch (type)
        {
            case PowerUpType.Shield:    return "SHIELD!";
            case PowerUpType.RapidFire: return "RAPID FIRE!";
            case PowerUpType.MultiShot: return "MULTI SHOT!";
            case PowerUpType.Health:    return "+1 HEALTH!";
            default: return "POWER UP!";
        }
    }
}
