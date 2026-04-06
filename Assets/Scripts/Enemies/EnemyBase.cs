using UnityEngine;

/// <summary>
/// Base class for all enemy types. Handles health, scoring, destruction,
/// and power-up drop logic. Derived classes implement specific movement/attack behaviors.
/// </summary>
public abstract class EnemyBase : MonoBehaviour, IPoolable
{
    [Header("Enemy Base Settings")]
    public int maxHealth = 1;
    public int scoreValue = 100;
    public float moveSpeed = 3f;
    public string poolTag = "Enemy";

    [Header("Shooting")]
    public bool canShoot = false;
    public float shootInterval = 2f;
    public float bulletSpeed = 6f;
    public string bulletPoolTag = "EnemyBullet";

    [Header("Power-Up Drop")]
    [Range(0f, 1f)]
    public float powerUpDropChance = 0.15f;

    protected int currentHealth;
    protected float shootTimer;
    protected Transform playerTransform;

    public virtual void OnSpawnFromPool()
    {
        currentHealth = maxHealth;
        shootTimer = shootInterval;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    protected virtual void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        Move();
        HandleShooting();
        CheckOutOfBounds();
    }

    /// <summary>
    /// Implement specific movement patterns in derived classes.
    /// </summary>
    protected abstract void Move();

    protected virtual void HandleShooting()
    {
        if (!canShoot) return;

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootInterval;
        }
    }

    protected virtual void Shoot()
    {
        if (ObjectPool.Instance == null) return;

        Vector2 direction = Vector2.down;
        if (playerTransform != null)
        {
            direction = (playerTransform.position - transform.position).normalized;
        }

        GameObject bullet = ObjectPool.Instance.Spawn(bulletPoolTag,
            transform.position + Vector3.down * 0.5f, Quaternion.identity);

        if (bullet != null)
        {
            Bullet bulletComp = bullet.GetComponent<Bullet>();
            if (bulletComp != null)
            {
                bulletComp.Initialize(direction, bulletSpeed, 1, false);
            }
        }

        AudioManager.Instance?.PlaySound("EnemyShoot");
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
            AudioManager.Instance?.PlaySound("EnemyHit");
        }
    }

    protected virtual void Die()
    {
        GameManager.Instance?.AddScore(scoreValue);
        GameManager.Instance?.EnemyDestroyed();
        AudioManager.Instance?.PlaySound("EnemyDeath");

        TryDropPowerUp();
        ReturnToPool();
    }

    private void TryDropPowerUp()
    {
        if (Random.value <= powerUpDropChance)
        {
            PowerUpSpawner spawner = FindObjectOfType<PowerUpSpawner>();
            if (spawner != null)
            {
                spawner.SpawnRandomPowerUp(transform.position);
            }
        }
    }

    protected void ReturnToPool()
    {
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void CheckOutOfBounds()
    {
        if (GameBounds.Instance != null && GameBounds.Instance.IsOutOfBounds(transform.position, 2f))
        {
            // Dont count score for enemies that leave the screen
            GameManager.Instance?.EnemyDestroyed();
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
            Die();
        }
    }
}
