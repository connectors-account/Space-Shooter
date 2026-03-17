using UnityEngine;

/// <summary>
/// Controls bullet movement, lifetime, and collision behavior.
/// Can be used for both player and enemy bullets.
/// </summary>
public class BulletController : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 25;
    [SerializeField] private bool isPlayerBullet = true;
    
    [Header("Visual")]
    [SerializeField] private Color playerBulletColor = Color.cyan;
    [SerializeField] private Color enemyBulletColor = Color.red;
    
    // State
    private Vector2 direction = Vector2.up;
    private float timeAlive;
    private SpriteRenderer spriteRenderer;
    
    public bool IsPlayerBullet => isPlayerBullet;
    public int Damage => damage;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Start()
    {
        UpdateVisual();
    }
    
    private void Update()
    {
        // Move bullet
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        
        // Check lifetime
        timeAlive += Time.deltaTime;
        if (timeAlive >= lifetime)
        {
            Destroy(gameObject);
        }
        
        // Destroy if off screen
        if (Mathf.Abs(transform.position.x) > 12f || Mathf.Abs(transform.position.y) > 8f)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initialize bullet with direction and ownership
    /// </summary>
    public void Initialize(Vector2 moveDirection, bool playerOwned)
    {
        direction = moveDirection.normalized;
        isPlayerBullet = playerOwned;
        UpdateVisual();
        
        // Rotate to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    /// <summary>
    /// Set bullet damage
    /// </summary>
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    /// <summary>
    /// Set bullet speed
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    private void UpdateVisual()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isPlayerBullet ? playerBulletColor : enemyBulletColor;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player bullet hitting enemy
        if (isPlayerBullet)
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                HealthSystem health = enemy.GetComponent<HealthSystem>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
        // Enemy bullet hitting player
        else
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.HasShield)
            {
                player.Health?.TakeDamage(damage);
                Destroy(gameObject);
            }
            else if (player != null && player.HasShield)
            {
                // Bullet destroyed by shield
                Destroy(gameObject);
            }
        }
    }
}
