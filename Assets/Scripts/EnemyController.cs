using UnityEngine;

/// <summary>
/// Controls enemy behavior: movement patterns, shooting, health, and drops.
/// Attach to Enemy GameObjects with Rigidbody2D and Collider2D.
/// </summary>
public class EnemyController : MonoBehaviour
{
    public enum MovementPattern { Straight, Zigzag, Sine, Dive }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private MovementPattern pattern = MovementPattern.Straight;
    [SerializeField] private float zigzagAmplitude = 2f;
    [SerializeField] private float zigzagFrequency = 2f;

    [Header("Shooting")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private float bulletSpeed = 6f;
    [SerializeField] private bool canShoot = true;

    [Header("Stats")]
    [SerializeField] private int health = 1;
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private float powerUpDropChance = 0.15f;

    [Header("References")]
    [SerializeField] private GameObject powerUpPrefab;

    // Internal state
    private float nextFireTime;
    private float spawnTime;
    private Vector3 startPosition;
    private float screenBottom;

    private void Start()
    {
        spawnTime = Time.time;
        startPosition = transform.position;

        Camera cam = Camera.main;
        if (cam != null)
        {
            screenBottom = cam.ScreenToWorldPoint(new Vector3(0, 0, cam.transform.position.z)).y - 1f;
        }
        else
        {
            screenBottom = -7f;
        }

        nextFireTime = Time.time + Random.Range(0.5f, fireRate);
    }

    private void Update()
    {
        HandleMovement();
        HandleShooting();
        CheckOffScreen();
    }

    /// <summary>
    /// Moves the enemy based on the selected movement pattern.
    /// </summary>
    private void HandleMovement()
    {
        float elapsed = Time.time - spawnTime;
        Vector3 pos = transform.position;

        switch (pattern)
        {
            case MovementPattern.Straight:
                pos.y -= moveSpeed * Time.deltaTime;
                break;

            case MovementPattern.Zigzag:
                pos.y -= moveSpeed * Time.deltaTime;
                pos.x = startPosition.x + Mathf.Sin(elapsed * zigzagFrequency) * zigzagAmplitude;
                break;

            case MovementPattern.Sine:
                pos.y -= moveSpeed * Time.deltaTime;
                pos.x = startPosition.x + Mathf.Sin(elapsed * zigzagFrequency) * zigzagAmplitude;
                break;

            case MovementPattern.Dive:
                // Dives toward the player if available, otherwise goes straight
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null && player.activeInHierarchy)
                {
                    Vector3 dir = (player.transform.position - pos).normalized;
                    pos += dir * moveSpeed * Time.deltaTime;
                }
                else
                {
                    pos.y -= moveSpeed * Time.deltaTime;
                }
                break;
        }

        transform.position = pos;
    }

    /// <summary>
    /// Fires bullets downward at the set fire rate.
    /// </summary>
    private void HandleShooting()
    {
        if (!canShoot || enemyBulletPrefab == null) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate + Random.Range(-0.3f, 0.3f);

        GameObject bullet = Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.Initialize(Vector2.down, bulletSpeed, false);
        }

        AudioManager.Instance?.PlaySFX("EnemyShoot");
    }

    /// <summary>
    /// Destroys the enemy if it moves off the bottom of the screen.
    /// </summary>
    private void CheckOffScreen()
    {
        if (transform.position.y < screenBottom)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Applies damage to the enemy. Destroys it if health reaches zero.
    /// </summary>
    public void TakeDamage(int damage)
    {
        health -= damage;

        // Flash red briefly
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            StartCoroutine(FlashDamage(sr));
        }

        if (health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Brief red flash when hit.
    /// </summary>
    private System.Collections.IEnumerator FlashDamage(SpriteRenderer sr)
    {
        Color original = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.08f);
        if (sr != null) sr.color = original;
    }

    /// <summary>
    /// Handles enemy death: awards score, spawns effects/drops, then destroys.
    /// </summary>
    private void Die()
    {
        AudioManager.Instance?.PlaySFX("EnemyDeath");

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

        // Chance to drop power-up
        if (Random.value < powerUpDropChance)
        {
            SpawnPowerUp();
        }

        // Simple explosion effect — spawn a brief expanding circle
        SpawnExplosionEffect();

        Destroy(gameObject);
    }

    /// <summary>
    /// Spawns a random power-up at the enemy's position.
    /// </summary>
    private void SpawnPowerUp()
    {
        if (powerUpPrefab != null)
        {
            Instantiate(powerUpPrefab, transform.position, Quaternion.identity);
        }
    }

    /// <summary>
    /// Creates a simple expanding circle explosion effect.
    /// </summary>
    private void SpawnExplosionEffect()
    {
        GameObject explosion = new GameObject("Explosion");
        explosion.transform.position = transform.position;
        SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(1f, 0.6f, 0.1f, 0.8f);
        sr.sortingOrder = 10;
        explosion.AddComponent<ExplosionEffect>();
    }

    private Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        float center = size / 2f;
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                colors[y * size + x] = dist <= radius ? Color.white : Color.clear;
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TakeDamage(health); // Destroy on contact
        }
    }

    // --- Configuration setters used by EnemySpawner ---
    public void SetMoveSpeed(float speed) => moveSpeed = speed;
    public void SetHealth(int hp) => health = hp;
    public void SetScoreValue(int score) => scoreValue = score;
    public void SetPattern(MovementPattern p) => pattern = p;
    public void SetFireRate(float rate) => fireRate = rate;
    public void SetCanShoot(bool shoot) => canShoot = shoot;
    public void SetPowerUpPrefab(GameObject prefab) => powerUpPrefab = prefab;
    public void SetEnemyBulletPrefab(GameObject prefab) => enemyBulletPrefab = prefab;
}
