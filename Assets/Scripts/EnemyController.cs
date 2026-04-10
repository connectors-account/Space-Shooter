using UnityEngine;

public enum EnemyType
{
    Basic,
    Fast,
    Tank
}

[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private EnemyType enemyType = EnemyType.Basic;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private int maxHealth = 15;
    [SerializeField] private int contactDamage = 20;
    [SerializeField] private int scoreValue = 100;

    [Header("Shooting")]
    [SerializeField] private bool canShoot = true;
    [SerializeField] private float shotCooldown = 1.5f;
    [SerializeField] private Transform firePoint;

    [Header("Fast Movement")]
    [SerializeField] private float zigzagFrequency = 6f;
    [SerializeField] private float zigzagAmplitude = 1.25f;

    [Header("Drop")]
    [Range(0f, 1f)]
    [SerializeField] private float powerUpDropChance = 0.18f;

    private int _currentHealth;
    private float _nextShotTime;
    private float _spawnTime;
    private Vector3 _spawnPosition;
    private ObjectPool _originPool;
    private EnemySpawner _spawner;

    public EnemyType Type => enemyType;
    public int ContactDamage => contactDamage;

    public void Initialize(ObjectPool pool, EnemySpawner spawner)
    {
        _originPool = pool;
        _spawner = spawner;
        _currentHealth = maxHealth;
        _nextShotTime = Time.time + Random.Range(0.2f, shotCooldown);
        _spawnTime = Time.time;
        _spawnPosition = transform.position;
    }

    private void OnEnable()
    {
        if (_currentHealth <= 0)
        {
            _currentHealth = maxHealth;
        }

        _spawnTime = Time.time;
        _spawnPosition = transform.position;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
        {
            return;
        }

        HandleMovement();

        if (canShoot && Time.time >= _nextShotTime)
        {
            Shoot();
            _nextShotTime = Time.time + shotCooldown;
        }

        if (transform.position.y < -6.5f)
        {
            ReturnToPool();
        }
    }

    private void HandleMovement()
    {
        Vector3 movement = Vector3.down * (moveSpeed * Time.deltaTime);

        if (enemyType == EnemyType.Fast)
        {
            float xOffset = Mathf.Sin((Time.time - _spawnTime) * zigzagFrequency) * zigzagAmplitude;
            Vector3 target = _spawnPosition + new Vector3(xOffset, transform.position.y - _spawnPosition.y, 0f);
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            transform.position += movement;
            return;
        }

        transform.position += movement;
    }

    private void Shoot()
    {
        ObjectPool bulletPool = GameManager.Instance.GetEnemyBulletPool();
        if (bulletPool == null)
        {
            return;
        }

        Vector3 shootPos = firePoint != null ? firePoint.position : transform.position;

        if (enemyType == EnemyType.Tank)
        {
            float[] spread = { -18f, 0f, 18f };
            foreach (float angle in spread)
            {
                EnemyBullet bullet = bulletPool.Get<EnemyBullet>();
                if (bullet == null)
                {
                    continue;
                }

                bullet.transform.position = shootPos;
                Vector2 dir = Quaternion.Euler(0f, 0f, angle) * Vector2.down;
                bullet.Initialize(bulletPool, dir.normalized, 10);
            }
        }
        else
        {
            EnemyBullet bullet = bulletPool.Get<EnemyBullet>();
            if (bullet == null)
            {
                return;
            }

            bullet.transform.position = shootPos;
            bullet.Initialize(bulletPool, Vector2.down, enemyType == EnemyType.Fast ? 8 : 10);
        }
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        _currentHealth -= damage;
        if (_currentHealth > 0)
        {
            return;
        }

        Die();
    }

    private void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpawnExplosion(transform.position);
            GameManager.Instance.ScoreManager.RegisterEnemyKill(scoreValue);
        }

        TryDropPowerUp();
        _spawner?.NotifyEnemyDestroyed(this);
        ReturnToPool();
    }

    private void TryDropPowerUp()
    {
        if (GameManager.Instance == null || Random.value > powerUpDropChance)
        {
            return;
        }

        ObjectPool pool = GameManager.Instance.GetPowerUpPool();
        PowerUp powerUp = pool != null ? pool.Get<PowerUp>() : null;
        if (powerUp == null)
        {
            return;
        }

        powerUp.transform.position = transform.position;
        powerUp.Initialize(pool);
    }

    private void ReturnToPool()
    {
        if (_originPool != null)
        {
            _originPool.Return(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerBullet bullet))
        {
            TakeDamage(bullet.Damage);
            bullet.Release();
        }
    }
}
