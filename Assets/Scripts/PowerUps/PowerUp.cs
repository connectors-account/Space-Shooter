using UnityEngine;

/// <summary>
/// Power-up pickup that applies effects to the player.
/// </summary>
public enum PowerUpType
{
    RapidFire,
    Shield,
    Health
}

public class PowerUp : MonoBehaviour
{
    [SerializeField] private PowerUpType type = PowerUpType.Health;
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private float bobAmplitude = 0.3f;
    [SerializeField] private float bobFrequency = 3f;
    [SerializeField] private float rotationSpeed = 90f;

    [Header("Effect Values")]
    [SerializeField] private int healAmount = 30;
    [SerializeField] private float rapidFireDuration = 6f;
    [SerializeField] private float shieldDuration = 8f;

    private float spawnTime;
    private float startX;

    public PowerUpType Type
    {
        get => type;
        set => type = value;
    }

    private void Start()
    {
        spawnTime = Time.time;
        startX = transform.position.x;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Fall and bob
        float elapsed = Time.time - spawnTime;
        float x = startX + Mathf.Sin(elapsed * bobFrequency) * bobAmplitude;
        float y = transform.position.y - fallSpeed * Time.deltaTime;
        transform.position = new Vector3(x, y, transform.position.z);

        // Rotate
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Destroy if off screen
        if (y < -6f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        ApplyEffect(player);
        Destroy(gameObject);
    }

    private void ApplyEffect(PlayerController player)
    {
        switch (type)
        {
            case PowerUpType.Health:
                player.Heal(healAmount);
                break;
            case PowerUpType.RapidFire:
                player.ActivateRapidFire(rapidFireDuration);
                break;
            case PowerUpType.Shield:
                player.ActivateShield(shieldDuration);
                break;
        }

        AudioManager.Instance?.PlaySFX("PowerUp");
        ScoreManager.Instance?.AddScore(25);
    }
}
