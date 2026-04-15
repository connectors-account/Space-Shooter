using UnityEngine;

/// <summary>
/// Health restore collectible that falls downward.
/// </summary>
public class PowerUp : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private int healthRestoreAmount = 25;

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        if (transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Heal(healthRestoreAmount);
        }

        Destroy(gameObject);
    }
}
