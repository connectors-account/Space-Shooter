using UnityEngine;

/// <summary>
/// A collectible power-up that drifts downward and grants the player either
/// rapid fire or a temporary shield when picked up.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { RapidFire, Shield }

    [Tooltip("Which bonus this pickup grants.")]
    public PowerUpType type = PowerUpType.RapidFire;
    [Tooltip("Downward drift speed in world units per second.")]
    public float fallSpeed = 2f;

    private void Update()
    {
        // Drift down so the player has to move to collect it.
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        // Clean up if it falls off the bottom of the screen.
        if (Camera.main != null)
        {
            float bottom = Camera.main.ViewportToWorldPoint(Vector3.zero).y;
            if (transform.position.y < bottom - 1f)
                Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
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

            if (UIManager.Instance != null)
                UIManager.Instance.ShowPowerUpBanner(type.ToString());
        }

        Destroy(gameObject);
    }
}
