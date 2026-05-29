using UnityEngine;

/// <summary>
/// Power-up item that floats downward. Collected by player on contact.
/// Tag as "PowerUp".
/// </summary>
public enum PowerUpType
{
    Shield,
    RapidFire,
    SpreadShot
}

public class PowerUpController : MonoBehaviour
{
    public PowerUpType powerUpType = PowerUpType.Shield;
    public float fallSpeed = 2f;
    public float bobAmplitude = 0.3f;
    public float bobFrequency = 3f;
    public float lifetime = 10f;

    private float spawnTime;
    private float spawnY;
    private float spawnX;

    void Start()
    {
        spawnTime = Time.time;
        spawnX = transform.position.x;
        spawnY = transform.position.y;

        // Set color based on type
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            switch (powerUpType)
            {
                case PowerUpType.Shield:
                    sr.color = new Color(0.3f, 0.7f, 1f, 1f); // Light blue
                    break;
                case PowerUpType.RapidFire:
                    sr.color = new Color(1f, 0.6f, 0f, 1f); // Orange
                    break;
                case PowerUpType.SpreadShot:
                    sr.color = new Color(0.5f, 1f, 0.3f, 1f); // Green
                    break;
            }
        }

        gameObject.tag = "PowerUp";
    }

    void Update()
    {
        float elapsed = Time.time - spawnTime;

        // Fall downward with a gentle horizontal bob
        float yPos = spawnY - fallSpeed * elapsed;
        float xBob = Mathf.Sin(elapsed * bobFrequency) * bobAmplitude;
        transform.position = new Vector3(spawnX + xBob, yPos, 0f);

        // Slow rotation for visual appeal
        transform.Rotate(0, 0, 90f * Time.deltaTime);

        // Destroy if off screen or expired
        if (elapsed > lifetime || yPos < -7f)
        {
            Destroy(gameObject);
        }
    }
}
