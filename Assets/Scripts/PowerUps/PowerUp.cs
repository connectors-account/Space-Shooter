using UnityEngine;

/// <summary>
/// Power-up pickup item. Falls downward and activates when collected by player.
/// Types: RapidFire, SpreadShot, Shield, Health
/// </summary>
public enum PowerUpType { RapidFire, SpreadShot, Shield, Health }

public class PowerUp : MonoBehaviour
{
    public PowerUpType powerUpType;
    public float fallSpeed = 2f;
    public float duration = 8f;
    public float lifetime = 10f;

    private float timer;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        timer = lifetime;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Randomize type if not set explicitly
        if (spriteRenderer != null)
        {
            // Randomly assign type
            powerUpType = (PowerUpType)Random.Range(0, 4);
            UpdateVisual();
        }
    }

    void Update()
    {
        // Fall down
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        // Rotate for visual flair
        transform.Rotate(0, 0, 90f * Time.deltaTime);

        // Lifetime
        timer -= Time.deltaTime;
        if (timer <= 0f || transform.position.y < -7f)
        {
            Destroy(gameObject);
        }

        // Blink when about to expire
        if (timer < 3f && spriteRenderer != null)
        {
            spriteRenderer.enabled = Mathf.Sin(Time.time * 10f) > 0;
        }
    }

    void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        switch (powerUpType)
        {
            case PowerUpType.RapidFire:
                spriteRenderer.color = Color.yellow;
                break;
            case PowerUpType.SpreadShot:
                spriteRenderer.color = new Color(1f, 0.5f, 0f); // Orange
                break;
            case PowerUpType.Shield:
                spriteRenderer.color = new Color(0.3f, 0.7f, 1f); // Light blue
                break;
            case PowerUpType.Health:
                spriteRenderer.color = Color.green;
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.ActivatePowerUp(powerUpType, duration);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayPowerUp();

            // Show pickup text
            if (UIManager.Instance != null)
                UIManager.Instance.ShowPowerUpText(powerUpType.ToString());

            Destroy(gameObject);
        }
    }
}
