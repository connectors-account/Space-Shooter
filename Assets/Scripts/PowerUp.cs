using UnityEngine;

/// <summary>
/// A health power-up that drifts downward. When the player touches it,
/// the player is healed and the power-up is consumed.
/// Attach to the PowerUp prefab.
/// </summary>
public class PowerUp : MonoBehaviour
{
    [Tooltip("Amount of health restored on pickup.")]
    public int healAmount = 30;

    [Tooltip("Downward drift speed.")]
    public float fallSpeed = 2f;

    [Tooltip("Spin speed for visual flair (degrees/sec).")]
    public float spinSpeed = 180f;

    void Update()
    {
        // Drift downward
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        // Spin for visual effect
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);

        // Destroy if off-screen
        if (transform.position.y < -7f)
            Destroy(gameObject);
    }

    /// <summary>
    /// When the player touches this power-up, heal them and destroy the pickup.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount);
            }

            Destroy(gameObject);
        }
    }
}
