using UnityEngine;

/// <summary>
/// Controls an enemy ship: moves downward, optionally sways side-to-side.
/// Destroys itself when it goes off the bottom of the screen.
/// Attach this script to the Enemy prefab.
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Downward speed in units per second.")]
    public float fallSpeed = 3f;

    [Tooltip("Horizontal sway amplitude (0 = straight down).")]
    public float swayAmount = 1.5f;

    [Tooltip("Horizontal sway frequency.")]
    public float swaySpeed = 2f;

    [Header("Score")]
    [Tooltip("Points the player earns for destroying this enemy.")]
    public int scoreValue = 100;

    [Header("Boundaries")]
    [Tooltip("Y position below which this enemy is destroyed (off-screen cleanup).")]
    public float lowerBound = -8f;

    // ---- Internal ----
    private float spawnX;
    private float timeAlive;

    // =========================================================================
    // Unity Lifecycle
    // =========================================================================

    private void Start()
    {
        spawnX = transform.position.x;
        timeAlive = 0f;
    }

    private void Update()
    {
        timeAlive += Time.deltaTime;

        // Move downward with a gentle sine-wave sway
        float newX = spawnX + Mathf.Sin(timeAlive * swaySpeed) * swayAmount;
        float newY = transform.position.y - fallSpeed * Time.deltaTime;

        transform.position = new Vector3(newX, newY, 0f);

        // Destroy when past the bottom of the screen
        if (transform.position.y < lowerBound)
        {
            Destroy(gameObject);
        }
    }
}
