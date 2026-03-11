using UnityEngine;

/// <summary>
/// Controls bullet movement and behavior.
/// Attach this script to bullet prefabs or it will be added automatically.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Bullet movement speed")]
    [SerializeField] private float speed = 10f;
    
    [Tooltip("Direction of movement")]
    [SerializeField] private Vector2 direction = Vector2.up;
    
    [Header("Damage Settings")]
    [Tooltip("Damage dealt on hit")]
    [SerializeField] private int damage = 10;
    
    [Header("Lifetime Settings")]
    [Tooltip("Maximum time before bullet is destroyed (seconds)")]
    [SerializeField] private float maxLifetime = 5f;
    
    [Tooltip("Y boundaries for auto-destruction")]
    [SerializeField] private float destroyBoundaryY = 7f;
    
    // Whether this is a player bullet (true) or enemy bullet (false)
    private bool isPlayerBullet = true;
    
    // Cached components
    private Rigidbody2D rb;
    
    // Timing
    private float spawnTime;
    
    /// <summary>
    /// Gets the damage value of this bullet.
    /// </summary>
    public int Damage => damage;
    
    /// <summary>
    /// Checks if this is a player bullet.
    /// </summary>
    public bool IsPlayerBullet => isPlayerBullet;
    
    /// <summary>
    /// Initialize components.
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }
    }
    
    /// <summary>
    /// Set initial velocity.
    /// </summary>
    private void Start()
    {
        spawnTime = Time.time;
        
        // Apply initial velocity
        if (rb != null)
        {
            rb.velocity = direction.normalized * speed;
        }
        
        // Destroy after max lifetime
        Destroy(gameObject, maxLifetime);
    }
    
    /// <summary>
    /// Check for boundaries.
    /// </summary>
    private void Update()
    {
        // Destroy if out of bounds
        if (Mathf.Abs(transform.position.y) > destroyBoundaryY ||
            Mathf.Abs(transform.position.x) > destroyBoundaryY + 3f)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initialize the bullet with specified parameters.
    /// </summary>
    /// <param name="moveDirection">Direction to move</param>
    /// <param name="moveSpeed">Movement speed</param>
    /// <param name="bulletDamage">Damage on hit</param>
    /// <param name="fromPlayer">Whether fired by player</param>
    public void Initialize(Vector2 moveDirection, float moveSpeed, int bulletDamage, bool fromPlayer)
    {
        direction = moveDirection.normalized;
        speed = moveSpeed;
        damage = bulletDamage;
        isPlayerBullet = fromPlayer;
        
        // Set tag based on source
        gameObject.tag = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";
        
        // Apply velocity immediately if Rigidbody2D exists
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }
    
    /// <summary>
    /// Set bullet damage.
    /// </summary>
    /// <param name="newDamage">New damage value</param>
    public void SetDamage(int newDamage)
    {
        damage = Mathf.Max(1, newDamage);
    }
    
    /// <summary>
    /// Set bullet speed.
    /// </summary>
    /// <param name="newSpeed">New speed value</param>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }
    
    /// <summary>
    /// Change bullet direction.
    /// </summary>
    /// <param name="newDirection">New movement direction</param>
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }
}
