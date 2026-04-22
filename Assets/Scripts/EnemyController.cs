using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum MovementType
    {
        Straight,
        Sine,
        Drift
    }

    [Header("Runtime Settings")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float horizontalSpeed = 0f;
    [SerializeField] private MovementType movementType = MovementType.Straight;

    [Header("Shooting")]
    [SerializeField] private bool canShoot = false;
    [SerializeField] private float fireCooldown = 1.8f;
    [SerializeField] private BulletController enemyBulletPrefab;

    [Header("Drops")]
    [SerializeField, Range(0f, 1f)] private float powerUpDropChance = 0.12f;
    [SerializeField] private PowerUpController[] powerUpPrefabs;

    private int currentHealth;
    private float nextFireTime;
    private float sineSeed;
    private bool removed;
    private System.Action<EnemyController, bool, int> onRemoved;

    private void Start()
    {
        currentHealth = maxHealth;
        nextFireTime = Time.time + Random.Range(0.5f, fireCooldown);
        sineSeed = Random.Range(0f, 99f);
    }

    public void Configure(
        int health,
        int points,
        float speed,
        float horizontal,
        bool shoots,
        float shootCooldown,
        MovementType pattern,
        System.Action<EnemyController, bool, int> removedCallback)
    {
        maxHealth = Mathf.Max(1, health);
        currentHealth = maxHealth;
        scoreValue = Mathf.Max(10, points);
        moveSpeed = Mathf.Max(0.5f, speed);
        horizontalSpeed = horizontal;
        canShoot = shoots;
        fireCooldown = Mathf.Max(0.35f, shootCooldown);
        movementType = pattern;
        onRemoved = removedCallback;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        Move();
        TryShoot();

        if (transform.position.y < -6.8f)
        {
            Remove(false);
        }
    }

    private void Move()
    {
        Vector3 velocity = Vector3.down * moveSpeed;

        if (movementType == MovementType.Sine)
        {
            float sine = Mathf.Sin((Time.time + sineSeed) * 2f) * 1.4f;
            velocity.x = sine;
        }
        else if (movementType == MovementType.Drift)
        {
            velocity.x = horizontalSpeed;
        }

        transform.position += velocity * Time.deltaTime;
    }

    private void TryShoot()
    {
        if (!canShoot || enemyBulletPrefab == null || Time.time < nextFireTime)
        {
            return;
        }

        nextFireTime = Time.time + fireCooldown;

        BulletController bullet = Instantiate(enemyBulletPrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
        bullet.Initialize(BulletController.BulletOwner.Enemy, Vector2.down, 1);
    }

    public void TakeDamage(int damage)
    {
        if (removed)
        {
            return;
        }

        currentHealth -= Mathf.Max(1, damage);

        if (currentHealth <= 0)
        {
            TryDropPowerUp();
            Remove(true);
        }
    }

    private void TryDropPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
        {
            return;
        }

        if (Random.value > powerUpDropChance)
        {
            return;
        }

        int index = Random.Range(0, powerUpPrefabs.Length);
        Instantiate(powerUpPrefabs[index], transform.position, Quaternion.identity);
    }

    private void Remove(bool killedByPlayer)
    {
        if (removed)
        {
            return;
        }

        removed = true;
        onRemoved?.Invoke(this, killedByPlayer, scoreValue);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(1);
            }

            Remove(false);
        }
    }
}
