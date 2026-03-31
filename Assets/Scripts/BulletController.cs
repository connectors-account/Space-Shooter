using UnityEngine;

/// <summary>
/// BulletController moves a bullet upward and destroys it when it leaves the screen.
/// Attach this script to the Bullet prefab.
/// </summary>
public class BulletController : MonoBehaviour
{
    [Tooltip("Speed at which the bullet travels upward")]
    public float speed = 12f;

    [Tooltip("Seconds before the bullet auto-destroys (cleanup safety net)")]
    public float lifetime = 3f;

    void Start()
    {
        // Automatically destroy the bullet after its lifetime expires
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move the bullet straight up every frame
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// When the bullet hits an enemy, destroy the enemy and the bullet, and add score.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Award points
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(100);
            }

            // Destroy the enemy and the bullet
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
