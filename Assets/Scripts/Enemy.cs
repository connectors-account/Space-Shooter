using UnityEngine;

/// <summary>
/// Enemy behaviour — movement patterns, shooting, health.
/// Attach to enemy prefab (Sprite + Rigidbody2D kinematic + BoxCollider2D isTrigger).
/// Tag as "Enemy".
/// </summary>
public class Enemy : MonoBehaviour
{
    public enum MovePattern { Straight, Zigzag, Sine, Dive }

    [Header("Stats")]
    public int health = 30;
    public int scoreValue = 100;
    public int contactDamage = 25;

    [Header("Movement")]
    public MovePattern pattern = MovePattern.Straight;
    public float moveSpeed = 3f;
    public float zigzagAmplitude = 2f;
    public float zigzagFrequency = 2f;

    [Header("Shooting")]
    public bool canShoot = true;
    public GameObject bulletPrefab;
    public float fireRate = 1.5f;
    public float bulletSpeed = 6f;

    [Header("Power-up Drop")]
    public GameObject powerUpPrefab;
    [Range(0f, 1f)] public float dropChance = 0.15f;

    float nextFireTime;
    float spawnX;
    float aliveTime;

    void Start()
    {
        spawnX = transform.position.x;
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);
    }

    void Update()
    {
        aliveTime += Time.deltaTime;
        Move();
        TryShoot();
        // Destroy if off screen bottom
        if (transform.position.y < -7f) Destroy(gameObject);
    }

    void Move()
    {
        Vector3 pos = transform.position;
        switch (pattern)
        {
            case MovePattern.Straight:
                pos.y -= moveSpeed * Time.deltaTime;
                break;
            case MovePattern.Zigzag:
                pos.y -= moveSpeed * Time.deltaTime;
                pos.x = spawnX + Mathf.PingPong(aliveTime * zigzagFrequency, zigzagAmplitude * 2f) - zigzagAmplitude;
                break;
            case MovePattern.Sine:
                pos.y -= moveSpeed * Time.deltaTime;
                pos.x = spawnX + Mathf.Sin(aliveTime * zigzagFrequency) * zigzagAmplitude;
                break;
            case MovePattern.Dive:
                pos.y -= moveSpeed * 1.5f * Time.deltaTime;
                if (PlayerController.Instance != null)
                {
                    float dir = Mathf.Sign(PlayerController.Instance.transform.position.x - pos.x);
                    pos.x += dir * moveSpeed * 0.5f * Time.deltaTime;
                }
                break;
        }
        transform.position = pos;
    }

    void TryShoot()
    {
        if (!canShoot || bulletPrefab == null) return;
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            GameObject b = Instantiate(bulletPrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
            Bullet bullet = b.GetComponent<Bullet>();
            if (bullet != null)
            {
                // Aim toward player or straight down
                Vector2 dir = Vector2.down;
                if (PlayerController.Instance != null && Random.value > 0.4f)
                {
                    dir = ((Vector2)(PlayerController.Instance.transform.position - transform.position)).normalized;
                }
                bullet.Init(dir, bulletSpeed, false);
            }
            SoundManager.Instance?.PlaySFX("EnemyShoot");
        }
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0) Die();
    }

    void Die()
    {
        GameManager.Instance?.AddScore(scoreValue);
        SoundManager.Instance?.PlaySFX("Explosion");

        // Chance to drop power-up
        if (powerUpPrefab != null && Random.value <= dropChance)
        {
            Instantiate(powerUpPrefab, transform.position, Quaternion.identity);
        }

        // Simple death flash — spawn a short-lived white sprite
        SpawnExplosionEffect();
        Destroy(gameObject);
    }

    void SpawnExplosionEffect()
    {
        GameObject fx = new GameObject("Explosion");
        fx.transform.position = transform.position;
        var sr = fx.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = new Color(1f, 0.6f, 0.1f, 0.9f);
        sr.sortingOrder = 10;
        fx.transform.localScale = Vector3.one * 1.5f;
        fx.AddComponent<AutoDestroy>().lifetime = 0.2f;
    }

    Sprite CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(4, 4);
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                tex.SetPixel(x, y, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            Bullet b = other.GetComponent<Bullet>();
            TakeDamage(b != null ? b.damage : 10);
            Destroy(other.gameObject);
        }
    }
}
