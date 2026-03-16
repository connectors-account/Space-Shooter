using UnityEngine;

/// <summary>
/// Bullet class handles bullet movement and behavior.
/// Used by both player and enemy bullets through object pooling.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private bool isPlayerBullet = true;

    [Header("Visual")]
    [SerializeField] private TrailRenderer trailRenderer;

    // Private variables
    private Vector2 direction = Vector2.up;
    private float lifeTimer;
    private Rigidbody2D rb;

    public int Damage => damage;
    public bool IsPlayerBullet => isPlayerBullet;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    private void OnEnable()
    {
        // Reset state when pulled from pool
        lifeTimer = lifetime;
        
        // Clear trail
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }

    private void Update()
    {
        // Don't update if game is paused
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        Move();
        UpdateLifetime();
    }

    /// <summary>
    /// Move the bullet in its direction
    /// </summary>
    private void Move()
    {
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
        else
        {
            transform.position += (Vector3)direction * speed * Time.deltaTime;
        }
    }

    /// <summary>
    /// Update lifetime and deactivate when expired
    /// </summary>
    private void UpdateLifetime()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Set the direction the bullet should travel
    /// </summary>
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }

    /// <summary>
    /// Set bullet speed
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    /// <summary>
    /// Set bullet damage
    /// </summary>
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    /// <summary>
    /// Check if bullet is out of screen bounds
    /// </summary>
    private void OnBecameInvisible()
    {
        gameObject.SetActive(false);
    }
}
