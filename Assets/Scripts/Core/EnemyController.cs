using UnityEngine;

/// <summary>
/// EnemyController - Controls enemy movement, health, and scoring.
/// Attach to enemy prefab. Tag as "Enemy". Needs Collider2D (trigger) and Rigidbody2D (kinematic).
/// </summary>
public class EnemyController : MonoBehaviour
{
    public enum MovePattern { StraightDown, Zigzag, Sine }

    [Header("Movement")]
    public MovePattern movePattern = MovePattern.StraightDown;
    public float moveSpeed = 3f;
    public float zigzagAmplitude = 3f;
    public float zigzagFrequency = 2f;

    [Header("Health & Score")]
    public int maxHealth = 1;
    public int currentHealth;
    public int scoreValue = 100;

    [Header("Shooting (Optional)")]
    public bool canShoot = false;
    public GameObject enemyBulletPrefab;
    public float shootInterval = 2f;
    private float shootTimer;

    [Header("Bounds")]
    public float destroyYPosition = -7f;

    private float startX;
    private float timeAlive = 0f;

    private void Start()
    {
        currentHealth = maxHealth;
        startX = transform.position.x;
        shootTimer = Random.Range(1f, shootInterval);
    }

    private void Update()
    {
        HandleMovement();
        HandleShooting();
        CheckBounds();
    }

    /// <summary>
    /// Move the enemy based on its assigned pattern.
    /// </summary>
    private void HandleMovement()
    {
        timeAlive += Time.deltaTime;

        switch (movePattern)
        {
            case MovePattern.StraightDown:
                transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
                break;

            case MovePattern.Zigzag:
                float zigzagX = Mathf.Sin(timeAlive * zigzagFrequency) * zigzagAmplitude * Time.deltaTime;
                transform.Translate(new Vector3(zigzagX, -moveSpeed * Time.deltaTime, 0f), Space.World);
                break;

            case MovePattern.Sine:
                float sineX = Mathf.Cos(timeAlive * zigzagFrequency) * zigzagAmplitude;
                float newX = startX + sineX;
                float newY = transform.position.y - moveSpeed * Time.deltaTime;
                transform.position = new Vector3(newX, newY, 0f);
                break;
        }
    }

    /// <summary>
    /// Enemies that can shoot fire downward at intervals.
    /// </summary>
    private void HandleShooting()
    {
        if (!canShoot || enemyBulletPrefab == null) return;

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            shootTimer = shootInterval;
            Vector3 spawnPos = transform.position + Vector3.down * 0.6f;
            GameObject bullet = Instantiate(enemyBulletPrefab, spawnPos, Quaternion.identity);
            BulletController bc = bullet.GetComponent<BulletController>();
            if (bc != null)
            {
                bc.bulletSpeed = 6f;
                bc.direction = Vector3.down;
            }
        }
    }

    /// <summary>
    /// Destroy the enemy if it goes off-screen.
    /// </summary>
    private void CheckBounds()
    {
        if (transform.position.y < destroyYPosition)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Apply damage. Destroy and award score when health reaches zero.
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnEnemyDestroyed(scoreValue);
            }
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(FlashRed());
        }
    }

    /// <summary>
    /// Brief red flash when hit but not destroyed.
    /// </summary>
    private System.Collections.IEnumerator FlashRed()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = original;
        }
    }

    /// <summary>
    /// If a player bullet hits this enemy, take damage.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }
}
