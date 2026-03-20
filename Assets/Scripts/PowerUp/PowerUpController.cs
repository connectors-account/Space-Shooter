using UnityEngine;

/// <summary>
/// Controls power-up behavior: drifts downward, collected by player on contact.
/// Types: WeaponUpgrade, Shield, Health.
/// Attach to PowerUp prefab GameObjects.
/// </summary>
public class PowerUpController : MonoBehaviour
{
    public enum PowerUpType
    {
        WeaponUpgrade,
        Shield,
        Health
    }

    [Header("Settings")]
    [SerializeField] private PowerUpType type = PowerUpType.Health;
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float bobAmplitude = 0.3f;
    [SerializeField] private float bobFrequency = 3f;
    [SerializeField] private int healAmount = 2;

    private float spawnTime;
    private float spawnX;

    private void Start()
    {
        spawnTime = Time.time;
        spawnX = transform.position.x;
    }

    private void Update()
    {
        float elapsed = Time.time - spawnTime;

        // Move downward with a gentle bob
        float newX = spawnX + Mathf.Sin(elapsed * bobFrequency) * bobAmplitude;
        float newY = transform.position.y - fallSpeed * Time.deltaTime;
        transform.position = new Vector3(newX, newY, 0f);

        // Gentle rotation
        transform.Rotate(0, 0, 90f * Time.deltaTime);

        // Destroy if off screen
        if (transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        // Apply power-up effect
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

        AudioManager.Instance?.PlaySFX("PowerUp");
        Destroy(gameObject);
    }
}
