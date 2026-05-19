using UnityEngine;

/// <summary>
/// Type 3: Heavy bomber that moves slowly, has high health,
/// and fires spread shots. High score value.
/// </summary>
public class EnemyBomber : EnemyBase
{
    [Header("Bomber Settings")]
    [SerializeField] private int spreadCount = 3;
    [SerializeField] private float spreadAngle = 30f;
    [SerializeField] private float hoverY = 3f;
    [SerializeField] private float horizontalDrift = 1.5f;

    private float timeAlive;
    private float hoverTargetY;
    private bool isHovering;

    protected override void Start()
    {
        base.Start();
        maxHealth = 8;
        currentHealth = 8;
        scoreValue = 300;
        moveSpeed = 2f;
        fireRate = 2.5f;
        powerUpDropChance = 0.5f;
        hoverTargetY = Random.Range(2f, 4f);
        timeAlive = Random.Range(0f, Mathf.PI * 2);
    }

    /// <summary>
    /// Descends to a hover position, then drifts side to side.
    /// </summary>
    protected override void Move()
    {
        timeAlive += Time.deltaTime;

        if (!isHovering)
        {
            // Descend to hover position
            float newY = Mathf.MoveTowards(transform.position.y, hoverTargetY, moveSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, newY, 0);

            if (Mathf.Abs(transform.position.y - hoverTargetY) < 0.1f)
                isHovering = true;
        }
        else
        {
            // Drift horizontally while hovering
            float xDrift = Mathf.Sin(timeAlive * 0.8f) * horizontalDrift * Time.deltaTime;
            // Slowly descend
            float yDrift = -0.3f * Time.deltaTime;
            transform.Translate(new Vector3(xDrift, yDrift, 0), Space.World);
        }
    }

    /// <summary>
    /// Fires a spread of bullets in a fan pattern downward.
    /// </summary>
    protected override void Shoot()
    {
        if (bulletPrefab == null) return;

        float startAngle = -spreadAngle / 2f;
        float angleStep = spreadCount > 1 ? spreadAngle / (spreadCount - 1) : 0f;

        for (int i = 0; i < spreadCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.down;

            GameObject bulletObj = Instantiate(bulletPrefab, transform.position + Vector3.down * 0.6f, Quaternion.identity);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.SetDirection(dir);
                bullet.SetSpeed(6f);
            }
        }

        if (shootSound != null && audioSource != null)
            audioSource.PlayOneShot(shootSound, 0.5f);
    }
}
