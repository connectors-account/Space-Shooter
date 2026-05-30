using UnityEngine;

/// <summary>
/// Bullet – Moves upward and destroys enemies on contact.
/// Attach to the Bullet prefab. Requires Rigidbody2D, BoxCollider2D (Is Trigger).
/// </summary>
public class Bullet : MonoBehaviour
{
    [Tooltip("Speed the bullet travels upward")]
    public float speed = 12f;

    [Tooltip("Points awarded when this bullet destroys an enemy")]
    public int scoreValue = 10;

    // Auto-destroy boundary (top of screen + margin)
    private float destroyY;

    void Start()
    {
        // Calculate top boundary from camera
        destroyY = Camera.main.orthographicSize + 1f;
    }

    void Update()
    {
        // Move upward
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        // Destroy when off-screen
        if (transform.position.y > destroyY)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Only interact with enemies
        if (other.CompareTag("Enemy"))
        {
            // Award score
            if (GameManager.Instance != null)
                GameManager.Instance.AddScore(scoreValue);

            // Update HUD
            if (HUDManager.Instance != null)
                HUDManager.Instance.UpdateScore(GameManager.Instance.Score);

            // Destroy both enemy and bullet
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
