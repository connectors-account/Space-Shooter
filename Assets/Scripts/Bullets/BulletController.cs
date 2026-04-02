using UnityEngine;

/// <summary>
/// Controls bullet movement and damage. Used by both player and enemy bullets.
/// </summary>
public class BulletController : MonoBehaviour
{
    [Header("Defaults (overridden by Initialize)")]
    [SerializeField] private float defaultSpeed = 10f;
    [SerializeField] private int defaultDamage = 1;
    [SerializeField] private bool defaultIsPlayerBullet = true;
    [SerializeField] private float lifetime = 5f;

    private Vector2 direction;
    private float speed;
    private int damage;
    private bool isPlayerBullet;
    private float spawnTime;
    private bool initialized;

    public bool IsPlayerBullet => isPlayerBullet;
    public int Damage => damage;

    private void Start()
    {
        spawnTime = Time.time;
        if (!initialized)
        {
            direction = defaultIsPlayerBullet ? Vector2.up : Vector2.down;
            speed = defaultSpeed;
            damage = defaultDamage;
            isPlayerBullet = defaultIsPlayerBullet;
        }

        // Rotate bullet sprite to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    /// <summary>
    /// Initialize bullet parameters at spawn time.
    /// </summary>
    public void Initialize(Vector2 dir, float spd, int dmg, bool playerBullet)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        isPlayerBullet = playerBullet;
        initialized = true;

        // Set layer for collision filtering
        gameObject.layer = LayerMask.NameToLayer(playerBullet ? "PlayerBullet" : "EnemyBullet");

        // Set tag
        gameObject.tag = playerBullet ? "PlayerBullet" : "EnemyBullet";

        // Rotate bullet sprite to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Set color based on owner
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = playerBullet ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.3f, 0.2f);
        }
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Destroy if out of bounds or expired
        if (Time.time - spawnTime > lifetime ||
            Mathf.Abs(transform.position.x) > 12f ||
            Mathf.Abs(transform.position.y) > 8f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet)
        {
            // Player bullet hitting enemy
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            // Enemy bullet hitting player
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }
}
