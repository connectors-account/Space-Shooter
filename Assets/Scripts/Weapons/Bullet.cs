using UnityEngine;

/// <summary>
/// Generic bullet/projectile component used by both player and enemy bullets.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Defaults (overridden by Initialize)")]
    [SerializeField] private float defaultSpeed = 10f;
    [SerializeField] private int defaultDamage = 10;
    [SerializeField] private bool defaultIsPlayerBullet = true;
    [SerializeField] private float lifetime = 5f;

    private Vector2 direction;
    private float speed;
    private int damage;
    private bool isPlayerBullet;
    private bool isInitialized;

    public bool IsPlayerBullet => isPlayerBullet;
    public int Damage => damage;

    /// <summary>
    /// Initialize bullet with direction, speed, ownership, and damage.
    /// </summary>
    public void Initialize(Vector2 dir, float spd, bool playerBullet, int dmg)
    {
        direction = dir.normalized;
        speed = spd;
        isPlayerBullet = playerBullet;
        damage = dmg;
        isInitialized = true;
    }

    private void Start()
    {
        if (!isInitialized)
        {
            direction = isPlayerBullet ? Vector2.up : Vector2.down;
            speed = defaultSpeed;
            damage = defaultDamage;
            isPlayerBullet = defaultIsPlayerBullet;
        }

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Destroy if out of bounds
        if (Mathf.Abs(transform.position.x) > 12f || Mathf.Abs(transform.position.y) > 8f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet)
        {
            // Player bullet hits enemy
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                SpawnHitEffect();
                Destroy(gameObject);
            }
        }
        else
        {
            // Enemy bullet hits player
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                SpawnHitEffect();
                Destroy(gameObject);
            }
        }
    }

    private void SpawnHitEffect()
    {
        // Create a small flash at impact point
        GameObject flash = new GameObject("BulletImpact");
        flash.transform.position = transform.position;
        SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircleSprite(8, Color.white);
        sr.sortingOrder = 10;
        flash.AddComponent<AutoDestroy>().lifetime = 0.15f;
        flash.transform.localScale = Vector3.one * 0.3f;
    }
}
