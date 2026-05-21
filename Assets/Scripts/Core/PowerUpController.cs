using UnityEngine;

/// <summary>
/// PowerUpController - Drifts downward and grants a power-up on player contact.
/// Attach to power-up prefabs. Tag as "PowerUp". Needs Collider2D (trigger) and Rigidbody2D (kinematic).
/// </summary>
public class PowerUpController : MonoBehaviour
{
    public enum PowerUpType { RapidFire, Shield }

    [Header("Power-Up Settings")]
    public PowerUpType type = PowerUpType.RapidFire;
    public float driftSpeed = 2f;
    public float destroyYPosition = -7f;

    [Header("Visual Bobbing")]
    public float bobAmplitude = 0.2f;
    public float bobFrequency = 3f;

    private float startY;
    private float timeAlive = 0f;

    private void Start()
    {
        startY = transform.position.y;
    }

    private void Update()
    {
        timeAlive += Time.deltaTime;

        // Drift downward with a gentle bob
        float bobOffset = Mathf.Sin(timeAlive * bobFrequency) * bobAmplitude;
        float newY = startY - (driftSpeed * timeAlive) + bobOffset;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (transform.position.y < destroyYPosition)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                ApplyPowerUp(player);
            }
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Grant the specific power-up effect to the player.
    /// </summary>
    private void ApplyPowerUp(PlayerController player)
    {
        switch (type)
        {
            case PowerUpType.RapidFire:
                player.ActivateRapidFire();
                break;
            case PowerUpType.Shield:
                player.ActivateShield();
                break;
        }
        Debug.Log($"Power-Up collected: {type}");
    }
}
