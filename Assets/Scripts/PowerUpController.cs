using UnityEngine;

/// <summary>
/// PowerUpController - Handles power-up drift and effect application.
/// Attach to power-up prefabs with Rigidbody2D, CircleCollider2D (trigger), and tag "PowerUp".
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class PowerUpController : MonoBehaviour
{
    public enum PowerUpType
    {
        HealthRestore,
        RapidFire
    }

    [Header("Power-Up Settings")]
    public PowerUpType type = PowerUpType.HealthRestore;
    public float driftSpeed = 2f;
    public float lifetime = 10f;
    public float bobAmplitude = 0.3f;
    public float bobFrequency = 2f;

    private Rigidbody2D rb;
    private float spawnY;
    private float aliveTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;

        gameObject.tag = "PowerUp";
    }

    private void Start()
    {
        spawnY = transform.position.y;
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        // Drift downward with a gentle bobbing motion
        aliveTime += Time.fixedDeltaTime;
        float bob = Mathf.Sin(aliveTime * bobFrequency) * bobAmplitude;
        rb.linearVelocity = new Vector2(bob, -driftSpeed);
    }

    /// <summary>
    /// Apply this power-up's effect to the player.
    /// </summary>
    public void ApplyEffect(PlayerController player)
    {
        if (player == null) return;

        switch (type)
        {
            case PowerUpType.HealthRestore:
                player.RestoreHealth();
                break;
            case PowerUpType.RapidFire:
                player.ActivateRapidFire();
                break;
        }
    }
}
