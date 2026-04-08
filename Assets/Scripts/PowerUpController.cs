using UnityEngine;

/// <summary>
/// Power-up types available in the game.
/// </summary>
public enum PowerUpType
{
    Health,     // Restores health
    RapidFire,  // Increases fire rate temporarily
    Shield      // Absorbs one hit
}

/// <summary>
/// Controls power-up behavior: floating down, bobbing animation, and auto-destruction.
/// </summary>
public class PowerUpController : MonoBehaviour
{
    [Header("Power-Up Settings")]
    public PowerUpType powerUpType = PowerUpType.Health;
    public float fallSpeed = 2f;
    public float lifetime = 10f;

    [Header("Visual")]
    public float bobAmplitude = 0.2f;
    public float bobFrequency = 3f;
    public float rotateSpeed = 90f;

    private float aliveTime = 0f;
    private float startY;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        startY = transform.position.y;
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameObject.tag = "PowerUp";

        // Color-code the power-up based on type
        if (spriteRenderer != null)
        {
            switch (powerUpType)
            {
                case PowerUpType.Health:
                    spriteRenderer.color = Color.green;
                    break;
                case PowerUpType.RapidFire:
                    spriteRenderer.color = Color.yellow;
                    break;
                case PowerUpType.Shield:
                    spriteRenderer.color = new Color(0.3f, 0.6f, 1f); // Light blue
                    break;
            }
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isPaused)
            return;

        // Fall downward
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        // Bob up and down
        float bob = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position += new Vector3(0f, bob * Time.deltaTime, 0f);

        // Rotate slowly for visual flair
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        // Lifetime management
        aliveTime += Time.deltaTime;

        // Flash when about to expire
        if (aliveTime > lifetime - 2f && spriteRenderer != null)
        {
            float alpha = Mathf.PingPong(Time.time * 5f, 1f) > 0.5f ? 1f : 0.3f;
            Color c = spriteRenderer.color;
            spriteRenderer.color = new Color(c.r, c.g, c.b, alpha);
        }

        if (aliveTime >= lifetime || transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }
}
