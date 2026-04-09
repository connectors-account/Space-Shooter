using UnityEngine;

/// <summary>
/// Simple enemy AI: moves downward toward the player and slightly tracks
/// the player's horizontal position.
/// Attach to enemy prefab.
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Tooltip("Downward speed.")]
    public float moveSpeed = 3f;

    [Tooltip("How strongly the enemy tracks the player horizontally (0 = none).")]
    public float trackingStrength = 1.5f;

    [Tooltip("Health points for this enemy.")]
    public int health = 1;

    [Tooltip("Points awarded when this enemy is destroyed.")]
    public int scoreValue = 10;

    // Cached reference to the player transform
    private Transform playerTransform;

    void Start()
    {
        // Find the player in the scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    void Update()
    {
        // Base movement: always move downward
        Vector3 direction = Vector3.down * moveSpeed;

        // Horizontal tracking toward the player
        if (playerTransform != null)
        {
            float xDiff = playerTransform.position.x - transform.position.x;
            direction += Vector3.right * xDiff * trackingStrength * 0.3f;
        }

        transform.Translate(direction * Time.deltaTime, Space.World);

        // Destroy if it goes off-screen below
        if (transform.position.y < -7f)
            Destroy(gameObject);
    }

    /// <summary>
    /// Called when a bullet or other damaging object hits this enemy.
    /// </summary>
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            // Award score
            if (GameManager.Instance != null)
                GameManager.Instance.AddScore(scoreValue);

            Destroy(gameObject);
        }
    }
}
