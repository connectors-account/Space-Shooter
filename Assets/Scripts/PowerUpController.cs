using UnityEngine;

/// <summary>
/// Defines the types of power-ups available in the game.
/// </summary>
public enum PowerUpType
{
    /// <summary>Increases weapon level (up to triple shot).</summary>
    WeaponUpgrade,
    /// <summary>Grants a shield that absorbs one hit.</summary>
    Shield,
    /// <summary>Restores 30 health points.</summary>
    HealthRestore
}

/// <summary>
/// Controls power-up behavior: floating downward, bobbing, and auto-destruction.
/// Attach to power-up prefabs with Rigidbody2D (kinematic), CircleCollider2D (trigger).
/// </summary>
public class PowerUpController : MonoBehaviour
{
    [Header("Power-Up Settings")]
    [SerializeField] private PowerUpType powerUpType = PowerUpType.WeaponUpgrade;
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float bobAmplitude = 0.3f;
    [SerializeField] private float bobFrequency = 3f;
    [SerializeField] private float lifetime = 10f;

    private float startY;
    private float timeAlive;
    private Camera mainCamera;
    private float screenBottom;

    /// <summary>The type of this power-up.</summary>
    public PowerUpType Type => powerUpType;

    private void Start()
    {
        mainCamera = Camera.main;
        startY = transform.position.y;

        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        screenBottom = bottomLeft.y - 1f;

        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        timeAlive += Time.deltaTime;

        // Move downward with a horizontal bobbing effect
        float newY = transform.position.y - fallSpeed * Time.deltaTime;
        float bobOffset = Mathf.Sin(timeAlive * bobFrequency) * bobAmplitude;
        transform.position = new Vector3(transform.position.x + bobOffset * Time.deltaTime, newY, 0);

        // Destroy if off-screen
        if (transform.position.y < screenBottom)
        {
            Destroy(gameObject);
        }

        // Flashing effect when about to expire (last 3 seconds)
        if (lifetime - timeAlive < 3f)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float alpha = Mathf.PingPong(Time.time * 5f, 1f) > 0.5f ? 1f : 0.4f;
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }
    }
}
