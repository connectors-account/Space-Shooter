using UnityEngine;

/// <summary>
/// Swooper enemy: enters from the side, swoops in an arc, and exits.
/// </summary>
public class EnemySwooper : EnemyBase
{
    [Header("Swooper Settings")]
    [SerializeField] private float arcRadius = 4f;
    [SerializeField] private float arcSpeed = 1.5f;

    private float angle;
    private Vector3 arcCenter;
    private bool movingRight;

    protected override void Start()
    {
        base.Start();
        maxHealth = 25;
        scoreValue = 150;
        moveSpeed = 4f;
        fireRate = 2f;
        contactDamage = 20;
        currentHealth = maxHealth;

        // Determine arc direction based on spawn position
        movingRight = transform.position.x < 0;
        angle = movingRight ? Mathf.PI : 0f;
        arcCenter = new Vector3(0, transform.position.y - 2f, 0);
    }

    protected override void Move()
    {
        float direction = movingRight ? -1f : 1f;
        angle += direction * arcSpeed * Time.deltaTime;

        float x = arcCenter.x + Mathf.Cos(angle) * arcRadius;
        float y = arcCenter.y + Mathf.Sin(angle) * arcRadius * 0.5f;

        // Also drift downward
        arcCenter.y -= 0.5f * Time.deltaTime;

        transform.position = new Vector3(x, y, transform.position.z);
    }
}
