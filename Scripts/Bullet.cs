using UnityEngine;

/// <summary>
/// Handles bullet movement and collision detection
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 1;
    [SerializeField] private bool isPlayerBullet = true;

    [Header("Visual")]
    [SerializeField] private Color playerBulletColor = Color.yellow;
    [SerializeField] private Color enemyBulletColor = Color.red;

    private Vector2 direction = Vector2.up;
    private float spawnTime;

    private void Start()
    {
        spawnTime = Time.time;

        // Set color based on bullet type
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = isPlayerBullet ? playerBulletColor : enemyBulletColor;
        }

        // Set appropriate tag
        gameObject.tag = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";

        // Default direction based on bullet type
        if (!isPlayerBullet)
        {
            direction = Vector2.down;
        }
    }

    private void Update()
    {
        // Move bullet
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        // Check lifetime
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
        }

        // Also destroy if way off screen
        if (Mathf.Abs(transform.position.y) > 10f || Mathf.Abs(transform.position.x) > 15f)
        {
            Destroy(gameObject);
        }
    }

    // Set bullet direction (called by enemy or player when spawning)
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
        
        // Rotate bullet to face direction of travel
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Set bullet speed
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    // Set bullet as player or enemy bullet
    public void SetIsPlayerBullet(bool isPlayer)
    {
        isPlayerBullet = isPlayer;
        gameObject.tag = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = isPlayerBullet ? playerBulletColor : enemyBulletColor;
        }
    }

    // Get damage value
    public int GetDamage()
    {
        return damage;
    }

    // Optional: Add trail effect
    private void CreateTrailEffect()
    {
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.1f;
            trail.startWidth = 0.1f;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = isPlayerBullet ? playerBulletColor : enemyBulletColor;
            trail.endColor = Color.clear;
        }
    }
}
