using UnityEngine;

/// <summary>
/// Power-up item that drifts downward and applies an effect when
/// collected by the player. Types: RapidFire, Shield, HealthRestore.
/// </summary>
public class PowerUp : MonoBehaviour
{
    public enum PowerUpType
    {
        RapidFire,
        Shield,
        HealthRestore
    }

    [Header("Power-Up Settings")]
    [SerializeField] private PowerUpType type = PowerUpType.RapidFire;
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private float bobAmplitude = 0.3f;
    [SerializeField] private float bobFrequency = 3f;
    [SerializeField] private AudioClip pickupSound;

    [Header("Health Restore")]
    [SerializeField] private int healthRestoreAmount = 2;

    private float spawnTime;
    private float startY;

    public PowerUpType Type => type;

    private void Start()
    {
        spawnTime = Time.time;
        startY = transform.position.y;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (GameManager.Instance.IsGamePaused) return;

        // Fall downward with a gentle bob
        float elapsed = Time.time - spawnTime;
        float bobOffset = Mathf.Sin(elapsed * bobFrequency) * bobAmplitude;
        float yPos = startY - fallSpeed * elapsed;

        transform.position = new Vector3(
            transform.position.x + bobOffset * Time.deltaTime,
            transform.position.y - fallSpeed * Time.deltaTime,
            0
        );

        // Destroy if off screen
        if (transform.position.y < -7f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        ApplyEffect(player);

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, 0.8f);

        Destroy(gameObject);
    }

    /// <summary>
    /// Applies the power-up effect based on its type.
    /// </summary>
    private void ApplyEffect(PlayerController player)
    {
        switch (type)
        {
            case PowerUpType.RapidFire:
                player.ActivateRapidFire();
                break;
            case PowerUpType.Shield:
                player.ActivateShield();
                break;
            case PowerUpType.HealthRestore:
                player.RestoreHealth(healthRestoreAmount);
                break;
        }
    }
}
