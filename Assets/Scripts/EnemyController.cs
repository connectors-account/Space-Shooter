using UnityEngine;

/// <summary>
/// Controls an enemy ship. Supports a few simple movement patterns, has a
/// small health pool, awards score when destroyed by the player, and cleans
/// itself up once it travels off the bottom of the screen.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{
    public enum MovementPattern { Straight, Sine, Diagonal }

    [Header("Stats")]
    [Tooltip("Hit points before the enemy is destroyed.")]
    [SerializeField] private int maxHealth = 50;

    [Tooltip("Points awarded to the player when this enemy is destroyed.")]
    [SerializeField] private int scoreValue = 10;

    [Header("Movement")]
    [Tooltip("Downward travel speed in world units per second.")]
    [SerializeField] private float speed = 3f;

    [Tooltip("Pattern used for horizontal motion.")]
    [SerializeField] private MovementPattern pattern = MovementPattern.Straight;

    [Tooltip("Horizontal amplitude used by the Sine pattern.")]
    [SerializeField] private float sineAmplitude = 2f;

    [Tooltip("Horizontal frequency used by the Sine pattern.")]
    [SerializeField] private float sineFrequency = 2f;

    [Tooltip("Horizontal direction (-1 left, 1 right) used by the Diagonal pattern.")]
    [SerializeField] private float diagonalDirection = 1f;

    [Header("Effects")]
    [Tooltip("Optional explosion/particle prefab spawned on death.")]
    [SerializeField] private GameObject deathEffect;

    private int currentHealth;
    private float startX;
    private float spawnTime;
    private Collider2D col;

    private void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        col = GetComponent<Collider2D>();
        col.isTrigger = true; // Trigger-based collisions throughout the game.
    }

    private void Start()
    {
        currentHealth = maxHealth;
        startX = transform.position.x;
        spawnTime = Time.time;

        // Randomise the diagonal direction if it was left at default for variety.
        if (pattern == MovementPattern.Diagonal && Mathf.Approximately(diagonalDirection, 0f))
        {
            diagonalDirection = Random.value < 0.5f ? -1f : 1f;
        }
    }

    private void Update()
    {
        MoveEnemy();
        DestroyIfBelowScreen();
    }

    /// <summary>Apply the configured movement pattern.</summary>
    private void MoveEnemy()
    {
        Vector3 pos = transform.position;

        // All patterns move downward over time.
        pos.y -= speed * Time.deltaTime;

        switch (pattern)
        {
            case MovementPattern.Sine:
                float elapsed = Time.time - spawnTime;
                pos.x = startX + Mathf.Sin(elapsed * sineFrequency) * sineAmplitude;
                break;

            case MovementPattern.Diagonal:
                pos.x += diagonalDirection * speed * 0.5f * Time.deltaTime;
                break;

            case MovementPattern.Straight:
            default:
                // No horizontal change.
                break;
        }

        transform.position = pos;
    }

    /// <summary>Remove the enemy once it has fully left the bottom of the view.</summary>
    private void DestroyIfBelowScreen()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 viewPos = cam.WorldToViewportPoint(transform.position);
        if (viewPos.y < -0.15f)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>Reduce health and die when it reaches zero.</summary>
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die(awardScore: true);
        }
    }

    /// <summary>Destroy this enemy, optionally awarding score and spawning an effect.</summary>
    public void Die(bool awardScore)
    {
        if (awardScore && GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
