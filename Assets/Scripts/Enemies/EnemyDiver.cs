using UnityEngine;

/// <summary>
/// Fast enemy that dives toward the player's position.
/// Doesn't shoot but deals high contact damage.
/// </summary>
public class EnemyDiver : EnemyBase
{
    private Vector3 targetPosition;
    private bool hasTarget = false;

    protected override void Awake()
    {
        base.Awake();
        maxHealth = 20;
        scoreValue = 120;
        moveSpeed = 6f;
        contactDamage = 30;
        powerUpDropChance = 0.1f;
    }

    private void Start()
    {
        // Lock onto player position at spawn
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.IsAlive)
        {
            targetPosition = player.transform.position;
            hasTarget = true;

            // Face the target
            Vector2 direction = (targetPosition - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            targetPosition = new Vector3(0, -6f, 0);
        }
    }

    protected override void Move()
    {
        // Move toward locked target position
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime, Space.Self);
    }

    protected override void Attack()
    {
        // Divers don't shoot, they ram
    }
}
