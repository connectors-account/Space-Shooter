using UnityEngine;

/// <summary>
/// Controls power-up behavior: floating down, bobbing, and applying effects on pickup.
/// Attach to HealthPowerUp and WeaponPowerUp prefabs.
/// </summary>
public class PowerUpController : MonoBehaviour
{
    public enum PowerUpType { Health, WeaponUpgrade, Shield }

    [Header("Settings")]
    public PowerUpType type = PowerUpType.Health;
    public float fallSpeed = 2f;
    public float bobAmplitude = 0.3f;
    public float bobFrequency = 3f;
    public float lifetime = 8f;

    [Header("Health Power-Up")]
    public int healAmount = 2;

    private float startX;
    private float elapsedTime;

    void Start()
    {
        startX = transform.position.x;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        // Fall down with a gentle bob
        float bobOffset = Mathf.Sin(elapsedTime * bobFrequency) * bobAmplitude;
        transform.position += new Vector3(0f, -fallSpeed * Time.deltaTime, 0f);

        // Slight horizontal bob
        float xBob = Mathf.Sin(elapsedTime * bobFrequency * 0.7f) * 0.02f;
        transform.position += new Vector3(xBob, 0f, 0f);

        // Visual pulse
        float scale = 1f + Mathf.Sin(elapsedTime * 4f) * 0.1f;
        transform.localScale = Vector3.one * 0.4f * scale;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        switch (type)
        {
            case PowerUpType.Health:
                player.Heal(healAmount);
                break;

            case PowerUpType.WeaponUpgrade:
                player.UpgradeWeapon();
                break;

            case PowerUpType.Shield:
                // Could add shield mechanic - for now heals 1
                player.Heal(1);
                break;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPowerUp();

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(50);

        Destroy(gameObject);
    }
}
