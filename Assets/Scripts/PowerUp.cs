using UnityEngine;

/// <summary>
/// PowerUp – Drifts downward. When collected by the player, applies a buff.
/// Two types: RapidFire (yellow) and Shield (cyan).
/// Attach to Power-Up prefabs. Requires Rigidbody2D, CircleCollider2D (Is Trigger).
/// Tag must be "PowerUp".
/// </summary>
public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { RapidFire, Shield }

    [Header("Power-Up Settings")]
    public PowerUpType type = PowerUpType.RapidFire;

    [Tooltip("Duration of the power-up effect in seconds")]
    public float duration = 5f;

    [Tooltip("Downward drift speed")]
    public float fallSpeed = 2f;

    // Auto-destroy
    private float destroyY;

    void Start()
    {
        destroyY = -(Camera.main.orthographicSize + 1f);
    }

    void Update()
    {
        // Drift downward with a gentle bob
        Vector3 pos = transform.position;
        pos.y -= fallSpeed * Time.deltaTime;
        // Small horizontal oscillation for visual flair
        pos.x += Mathf.Sin(Time.time * 3f) * 0.01f;
        transform.position = pos;

        if (pos.y < destroyY)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Only the player can collect power-ups
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        switch (type)
        {
            case PowerUpType.RapidFire:
                player.ActivateRapidFire(duration);
                break;
            case PowerUpType.Shield:
                player.ActivateShield(duration);
                break;
        }

        Destroy(gameObject);
    }
}
