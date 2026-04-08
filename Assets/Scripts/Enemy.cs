using UnityEngine;

/// <summary>
/// Base enemy behaviour: moves downward, can shoot, takes damage, drops power-ups.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public int   maxHealth   = 2;
    public int   scoreValue  = 100;
    public float moveSpeed   = 3f;

    [Header("Shooting")]
    public bool  canShoot    = false;
    public float fireRate    = 2f;
    public GameObject bulletPrefab;
    public float bulletSpeed = 6f;

    [Header("Movement Pattern")]
    public MovementType movementType = MovementType.Straight;
    public float sineAmplitude = 2f;
    public float sineFrequency = 2f;

    [Header("Power-Up Drop")]
    [Range(0f, 1f)] public float dropChance = 0.15f;

    public enum MovementType { Straight, Sine, Diagonal }

    private int   currentHealth;
    private float nextFireTime;
    private float startX;
    private float spawnTime;

    void Start()
    {
        currentHealth = maxHealth;
        startX = transform.position.x;
        spawnTime = Time.time;
        gameObject.tag = "Enemy";
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        Move();
        TryShoot();
        DestroyIfOffScreen();
    }

    void Move()
    {
        float elapsed = Time.time - spawnTime;
        Vector3 pos = transform.position;

        switch (movementType)
        {
            case MovementType.Straight:
                pos.y -= moveSpeed * Time.deltaTime;
                break;

            case MovementType.Sine:
                pos.y -= moveSpeed * Time.deltaTime;
                pos.x = startX + Mathf.Sin(elapsed * sineFrequency) * sineAmplitude;
                break;

            case MovementType.Diagonal:
                pos.y -= moveSpeed * Time.deltaTime;
                pos.x += moveSpeed * 0.5f * Time.deltaTime * (startX > 0 ? -1f : 1f);
                break;
        }

        transform.position = pos;
    }

    void TryShoot()
    {
        if (!canShoot || bulletPrefab == null) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate;
        GameObject bullet = Instantiate(bulletPrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.isPlayerBullet = false;
            b.SetDirection(Vector2.down, bulletSpeed);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        // Flash white briefly
        StartCoroutine(FlashWhite());

        if (currentHealth <= 0)
            Die();
    }

    System.Collections.IEnumerator FlashWhite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color orig = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            if (sr != null) sr.color = orig;
        }
    }

    void Die()
    {
        // Add score
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

        // Maybe drop a power-up
        if (Random.value <= dropChance)
            PowerUpSpawner.SpawnRandomAt(transform.position);

        AudioManager.PlaySfx("EnemyExplosion");
        Destroy(gameObject);
    }

    void DestroyIfOffScreen()
    {
        if (transform.position.y < -7f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            Bullet b = other.GetComponent<Bullet>();
            int dmg = b != null ? b.damage : 1;
            TakeDamage(dmg);
            Destroy(other.gameObject);
        }
    }
}
