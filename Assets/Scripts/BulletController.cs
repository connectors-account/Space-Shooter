using UnityEngine;

/// <summary>
/// Moves a bullet upward and destroys it when it leaves the screen.
/// Attach this script to the Bullet prefab.
/// </summary>
public class BulletController : MonoBehaviour
{
    [Header("Bullet Settings")]
    [Tooltip("Speed of the bullet in units per second.")]
    public float speed = 15f;

    [Tooltip("Y position beyond which the bullet is destroyed (off-screen cleanup).")]
    public float upperBound = 10f;

    // =========================================================================
    // Unity Lifecycle
    // =========================================================================

    private void Update()
    {
        // Move straight up
        transform.position += Vector3.up * speed * Time.deltaTime;

        // Destroy when off-screen
        if (transform.position.y > upperBound)
        {
            Destroy(gameObject);
        }
    }

    // =========================================================================
    // Collision
    // =========================================================================

    /// <summary>
    /// When the bullet hits an enemy, destroy both and award score.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Get the point value from the enemy (defaults to 100)
            EnemyController enemy = other.GetComponent<EnemyController>();
            int points = (enemy != null) ? enemy.scoreValue : 100;

            // Award score
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(points);
            }

            // Destroy the enemy and this bullet
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
