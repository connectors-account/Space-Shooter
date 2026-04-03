using UnityEngine;

/// <summary>
/// Base class for all enemy types. Handles movement patterns and shooting.
/// </summary>
public class EnemyBase : MonoBehaviour
{
    public enum EnemyType { Straight, Zigzag, Swooper, Boss }

    [Header("Movement")]
    public EnemyType enemyType = EnemyType.Straight;
    public float moveSpeed = 3f;
    public float zigzagAmplitude = 2f;
    public float zigzagFrequency = 2f;

    [Header("Shooting")]
    public GameObject enemyBulletPrefab;
    public float shootInterval = 2f;
    public float bulletSpeed = 6f;
    public bool canShoot = true;

    [Header("Score")]
    public int scoreValue = 100;

    private float shootTimer;
    private float startX;
    private float moveTimer;
    private int swoopPhase = 0;
    private Vector3 swoopTarget;

    void Start()
    {
        startX = transform.position.x;
        shootTimer = Random.Range(0.5f, shootInterval);
        moveTimer = Random.Range(0f, Mathf.PI * 2f); // Random phase offset

        if (enemyType == EnemyType.Swooper)
        {
            swoopTarget = new Vector3(
                Random.Range(-6f, 6f),
                Random.Range(-1f, 2f),
                0f
            );
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive()) return;

        HandleMovement();
        HandleShooting();
        CheckBounds();
    }

    void HandleMovement()
    {
        moveTimer += Time.deltaTime;

        switch (enemyType)
        {
            case EnemyType.Straight:
                transform.position += Vector3.down * moveSpeed * Time.deltaTime;
                break;

            case EnemyType.Zigzag:
                float xOffset = Mathf.Sin(moveTimer * zigzagFrequency) * zigzagAmplitude;
                transform.position = new Vector3(
                    startX + xOffset,
                    transform.position.y - moveSpeed * Time.deltaTime,
                    0f
                );
                break;

            case EnemyType.Swooper:
                HandleSwoopMovement();
                break;

            case EnemyType.Boss:
                // Boss moves slowly side to side at top of screen
                float bossX = Mathf.Sin(moveTimer * 0.5f) * 4f;
                transform.position = new Vector3(
                    bossX,
                    transform.position.y,
                    0f
                );
                break;
        }
    }

    void HandleSwoopMovement()
    {
        if (swoopPhase == 0)
        {
            // Move toward swoop target
            transform.position = Vector3.MoveTowards(
                transform.position, swoopTarget, moveSpeed * Time.deltaTime
            );
            if (Vector3.Distance(transform.position, swoopTarget) < 0.1f)
                swoopPhase = 1;
        }
        else
        {
            // Exit downward
            transform.position += Vector3.down * moveSpeed * 1.5f * Time.deltaTime;
        }
    }

    void HandleShooting()
    {
        if (!canShoot || enemyBulletPrefab == null) return;

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            shootTimer = shootInterval;
            Shoot();
        }
    }

    void Shoot()
    {
        if (enemyType == EnemyType.Boss)
        {
            // Boss fires spread pattern
            for (int i = -2; i <= 2; i++)
            {
                float angle = i * 15f;
                Quaternion rotation = Quaternion.Euler(0, 0, angle);
                Vector3 dir = rotation * Vector3.down;
                SpawnBullet(dir);
            }
        }
        else
        {
            // Normal enemies fire downward, optionally aimed
            Vector3 dir = Vector3.down;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && Random.value > 0.5f)
            {
                dir = (player.transform.position - transform.position).normalized;
            }
            SpawnBullet(dir);
        }
    }

    void SpawnBullet(Vector3 direction)
    {
        Vector3 spawnPos = transform.position + Vector3.down * 0.5f;
        GameObject bullet = Instantiate(enemyBulletPrefab, spawnPos, Quaternion.identity);
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.speed = bulletSpeed;
            b.direction = direction;
            b.isPlayerBullet = false;
        }
    }

    void CheckBounds()
    {
        if (transform.position.y < -7f || transform.position.y > 10f ||
            Mathf.Abs(transform.position.x) > 12f)
        {
            Destroy(gameObject);
        }
    }
}
