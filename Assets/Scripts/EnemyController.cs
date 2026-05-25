using UnityEngine;

/// <summary>
/// Controls enemy movement patterns, health, shooting, and death.
/// Attach to Enemy prefab GameObjects.
/// </summary>
public class EnemyController : MonoBehaviour
{
    public enum MovementPattern { Straight, Zigzag, Sine, Dive }

    [Header("Movement")]
    public MovementPattern pattern = MovementPattern.Straight;
    public float moveSpeed = 3f;
    public float zigzagAmplitude = 2f;
    public float zigzagFrequency = 2f;

    [Header("Health")]
    public int maxHealth = 2;
    public int currentHealth;

    [Header("Scoring")]
    public int scoreValue = 100;

    [Header("Shooting")]
    public bool canShoot = false;
    public GameObject enemyBulletPrefab;
    public float shootInterval = 2f;
    private float shootTimer;

    [Header("Drops")]
    [Range(0f, 1f)]
    public float powerUpDropChance = 0.15f;

    private float startX;
    private float elapsedTime;
    private bool isDiving = false;
    private Transform playerTransform;

    void Start()
    {
        currentHealth = maxHealth;
        startX = transform.position.x;
        shootTimer = Random.Range(0.5f, shootInterval);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isGameActive)
        {
            Destroy(gameObject);
            return;
        }

        elapsedTime += Time.deltaTime;
        HandleMovement();
        HandleShooting();
        DestroyIfOutOfBounds();
    }

    void HandleMovement()
    {
        switch (pattern)
        {
            case MovementPattern.Straight:
                transform.position += Vector3.down * moveSpeed * Time.deltaTime;
                break;

            case MovementPattern.Zigzag:
                float xOffset = Mathf.Sin(elapsedTime * zigzagFrequency) * zigzagAmplitude;
                transform.position = new Vector3(
                    startX + xOffset,
                    transform.position.y - moveSpeed * Time.deltaTime,
                    0f
                );
                break;

            case MovementPattern.Sine:
                float sineX = Mathf.Sin(elapsedTime * zigzagFrequency) * zigzagAmplitude * 0.5f;
                float sineY = Mathf.Cos(elapsedTime * zigzagFrequency * 0.5f) * 0.3f;
                transform.position += new Vector3(sineX * Time.deltaTime, (-moveSpeed + sineY) * Time.deltaTime, 0f);
                break;

            case MovementPattern.Dive:
                if (!isDiving)
                {
                    transform.position += Vector3.down * moveSpeed * 0.5f * Time.deltaTime;
                    if (elapsedTime > 1.5f) isDiving = true;
                }
                else
                {
                    // Dive towards player
                    if (playerTransform != null && playerTransform.gameObject.activeInHierarchy)
                    {
                        Vector3 dir = (playerTransform.position - transform.position).normalized;
                        transform.position += dir * moveSpeed * 1.5f * Time.deltaTime;
                    }
                    else
                    {
                        transform.position += Vector3.down * moveSpeed * 1.5f * Time.deltaTime;
                    }
                }
                break;
        }
    }

    void HandleShooting()
    {
        if (!canShoot || enemyBulletPrefab == null) return;

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            shootTimer = shootInterval + Random.Range(-0.3f, 0.3f);
            GameObject bullet = Instantiate(enemyBulletPrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
            BulletController bc = bullet.GetComponent<BulletController>();
            if (bc != null)
            {
                bc.isPlayerBullet = false;
                bc.speed = 6f;
                bc.direction = Vector3.down;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Flash red briefly
            StartCoroutine(FlashRed());
        }
    }

    System.Collections.IEnumerator FlashRed()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            sr.color = original;
        }
    }

    void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
            GameManager.Instance.EnemyDestroyed();
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayExplosion();

        // Chance to drop power-up
        if (Random.value < powerUpDropChance && GameManager.Instance != null)
        {
            GameManager.Instance.SpawnPowerUp(transform.position);
        }

        // Spawn explosion effect
        SpawnExplosionEffect();

        Destroy(gameObject);
    }

    void SpawnExplosionEffect()
    {
        // Create a simple particle burst using a temporary object
        GameObject explosion = new GameObject("Explosion");
        explosion.transform.position = transform.position;
        SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
        sr.color = new Color(1f, 0.5f, 0f, 1f); // Orange
        sr.sortingOrder = 10;
        // Use a default sprite - will be a square that fades
        sr.sprite = Resources.Load<Sprite>("Square");
        explosion.transform.localScale = Vector3.one * 0.5f;
        ExplosionEffect fx = explosion.AddComponent<ExplosionEffect>();
        fx.duration = 0.3f;
    }

    void DestroyIfOutOfBounds()
    {
        if (transform.position.y < -7f || transform.position.y > 8f ||
            Mathf.Abs(transform.position.x) > 12f)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.EnemyDestroyed();
            Destroy(gameObject);
        }
    }
}
