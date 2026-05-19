using UnityEngine;

/// <summary>
/// Type 2: Fighter enemy that moves in a zigzag pattern and shoots at the player.
/// Medium health, medium score value, aimed shots.
/// </summary>
public class EnemyFighter : EnemyBase
{
    [Header("Fighter Settings")]
    [SerializeField] private float zigzagWidth = 3f;
    [SerializeField] private float zigzagSpeed = 2f;
    [SerializeField] private float descendSpeed = 1.5f;

    private float startX;
    private float timeAlive;
    private Transform playerTransform;

    protected override void Start()
    {
        base.Start();
        maxHealth = 3;
        currentHealth = 3;
        scoreValue = 150;
        moveSpeed = 2f;
        fireRate = 2f;
        powerUpDropChance = 0.25f;
        startX = transform.position.x;
        timeAlive = Random.Range(0f, Mathf.PI * 2);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    /// <summary>
    /// Zigzag descent pattern: moves side to side while descending.
    /// </summary>
    protected override void Move()
    {
        timeAlive += Time.deltaTime * zigzagSpeed;

        float targetX = startX + Mathf.Sin(timeAlive) * zigzagWidth;
        float newX = Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * 3f);
        float newY = transform.position.y - descendSpeed * Time.deltaTime;

        transform.position = new Vector3(newX, newY, 0);
    }

    /// <summary>
    /// Shoots aimed bullets toward the player's current position.
    /// </summary>
    protected override void Shoot()
    {
        if (bulletPrefab == null) return;

        Vector2 shootDir = Vector2.down;

        if (playerTransform != null)
        {
            shootDir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        }

        GameObject bulletObj = Instantiate(bulletPrefab, transform.position + Vector3.down * 0.4f, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetDirection(shootDir);
            bullet.SetSpeed(8f);
        }

        if (shootSound != null && audioSource != null)
            audioSource.PlayOneShot(shootSound, 0.4f);
    }
}
