using UnityEngine;

/// <summary>
/// BulletController moves a bullet in a given direction at a set speed.
/// It self-destructs after a lifetime expires or when it leaves the screen.
/// Both player and enemy bullets use this same script.
/// </summary>
public class BulletController : MonoBehaviour
{
    // ============================================================
    // CONFIGURATION
    // ============================================================

    [Tooltip("Direction the bullet travels (normalized)")]
    public Vector2 direction = Vector2.up;

    [Tooltip("Speed in units per second")]
    public float speed = 10f;

    [Tooltip("Damage dealt on hit")]
    public int damage = 1;

    [Tooltip("Seconds before the bullet auto-destroys")]
    public float lifetime = 5f;

    // ============================================================
    // INTERNAL
    // ============================================================
    private float spawnTime;

    // ============================================================
    // UNITY LIFECYCLE
    // ============================================================

    void Start()
    {
        spawnTime = Time.time;
    }

    void Update()
    {
        // Move the bullet each frame
        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);

        // Destroy after lifetime expires
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // Also destroy if far off-screen (safety net)
        if (Mathf.Abs(transform.position.x) > 12f || Mathf.Abs(transform.position.y) > 8f)
        {
            Destroy(gameObject);
        }
    }
}
