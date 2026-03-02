using UnityEngine;

/// <summary>
/// Bullet script handles bullet movement and collision detection.
/// Used for both player and enemy bullets.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [Tooltip("Movement speed of the bullet")]
    public float speed = 10f;
    
    [Tooltip("Damage dealt by this bullet")]
    public int damage = 10;
    
    [Tooltip("Time before bullet auto-destroys")]
    public float lifetime = 5f;
    
    [Tooltip("Is this a player bullet?")]
    private bool isPlayerBullet = true;
    
    private Vector2 direction = Vector2.up;
    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
    }

    void Update()
    {
        // Move bullet in specified direction
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        
        // Auto-destroy after lifetime
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
        }
        
        // Destroy if off-screen
        if (Mathf.Abs(transform.position.x) > 15f || Mathf.Abs(transform.position.y) > 10f)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Set the direction the bullet travels
    /// </summary>
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
        
        // Rotate bullet to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Set bullet speed
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    /// <summary>
    /// Set whether this is a player bullet or enemy bullet
    /// </summary>
    public void SetIsPlayerBullet(bool value)
    {
        isPlayerBullet = value;
        
        // Change tag and layer based on bullet type
        gameObject.tag = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";
        
        // Optionally change color to differentiate
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = isPlayerBullet ? Color.cyan : Color.red;
        }
    }

    /// <summary>
    /// Set bullet damage
    /// </summary>
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    /// <summary>
    /// Check if this is a player bullet
    /// </summary>
    public bool IsPlayerBullet()
    {
        return isPlayerBullet;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet)
        {
            // Player bullet hits enemy
            if (other.CompareTag("Enemy"))
            {
                EnemyBase enemy = other.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
        else
        {
            // Enemy bullet hits player
            if (other.CompareTag("Player"))
            {
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
    }
}
