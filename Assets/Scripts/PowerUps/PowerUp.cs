using UnityEngine;

/// <summary>
/// Types of power-ups available.
/// </summary>
public enum PowerUpType
{
    WeaponUpgrade,
    Shield,
    HealthPack
}

/// <summary>
/// Power-up pickup that drifts downward and applies effects to the player.
/// </summary>
public class PowerUp : MonoBehaviour
{
    [SerializeField] private PowerUpType type = PowerUpType.WeaponUpgrade;
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private int healAmount = 30;

    private float lifetimeTimer;
    private float bobOffset;

    public PowerUpType Type => type;

    public void Initialize(PowerUpType powerUpType)
    {
        type = powerUpType;
        lifetimeTimer = lifetime;
        bobOffset = Random.Range(0f, Mathf.PI * 2f);

        // Set color based on type
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            switch (type)
            {
                case PowerUpType.WeaponUpgrade:
                    sr.color = new Color(1f, 0.6f, 0f); // Orange
                    break;
                case PowerUpType.Shield:
                    sr.color = new Color(0.3f, 0.7f, 1f); // Light blue
                    break;
                case PowerUpType.HealthPack:
                    sr.color = new Color(0.2f, 1f, 0.2f); // Green
                    break;
            }
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

        // Fall downward with slight bob
        float bob = Mathf.Sin((Time.time + bobOffset) * 3f) * 0.5f;
        transform.Translate(new Vector3(bob * Time.deltaTime, -fallSpeed * Time.deltaTime, 0), Space.World);

        // Rotate
        transform.Rotate(Vector3.forward, 90f * Time.deltaTime);

        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f || transform.position.y < -6f)
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
            case PowerUpType.HealthPack:
                player.Heal(healAmount);
                break;
        }
    }
}
