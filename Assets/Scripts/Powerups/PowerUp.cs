using UnityEngine;

/// <summary>
/// Power-up collectible that drifts downward and applies effects to the player.
/// Attach to power-up prefabs.
/// </summary>
public class PowerUp : MonoBehaviour
{
    public enum PowerUpType
    {
        RapidFire,
        Shield,
        HealthRestore
    }

    public PowerUpType type = PowerUpType.RapidFire;
    public float fallSpeed = 2f;
    public float lifetime = 10f;
    public int healthRestoreAmount = 2;

    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
    }

    void Update()
    {
        // Drift downward
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        // Slight horizontal bobbing for visibility
        float bob = Mathf.Sin(Time.time * 3f) * 0.5f * Time.deltaTime;
        transform.Translate(new Vector3(bob, 0, 0), Space.World);

        // Destroy if lifetime exceeded or off-screen
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
        }

        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewPos.y < -0.1f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

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

        Destroy(gameObject);
    }
}
