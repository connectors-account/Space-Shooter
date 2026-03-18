using UnityEngine;

/// <summary>
/// Power-up item that drifts downward and is collected by the player on contact.
/// Attach this to the PowerUp prefab(s).
/// </summary>
public class PowerUp : MonoBehaviour
{
    public enum PowerUpType
    {
        Health,
        RapidFire
    }

    [Header("Power-Up Configuration")]
    public PowerUpType type = PowerUpType.Health;
    public float driftSpeed = 2f;
    public int healAmount = 2; // Only used for Health type

    void Update()
    {
        // Drift downward so the player can catch it
        transform.position += Vector3.down * driftSpeed * Time.deltaTime;

        // Optional: gentle bobbing motion for visual appeal
        float bob = Mathf.Sin(Time.time * 4f) * 0.3f;
        transform.position += Vector3.right * bob * Time.deltaTime;

        // Destroy if it goes off-screen
        if (transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
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
    /// Applies the power-up effect to the player.
    /// </summary>
    void ApplyPowerUp(PlayerController player)
    {
        switch (type)
        {
            case PowerUpType.Health:
                player.Heal(healAmount);
                Debug.Log("Health power-up collected! +" + healAmount + " HP");
                break;

            case PowerUpType.RapidFire:
                player.ActivateRapidFire();
                Debug.Log("Rapid fire power-up collected!");
                break;
        }
    }
}
