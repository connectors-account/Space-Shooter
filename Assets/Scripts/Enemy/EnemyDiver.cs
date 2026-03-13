using UnityEngine;

/// <summary>
/// Enemy that dives toward the player's position, then continues past.
/// </summary>
public class EnemyDiver : EnemyBase
{
    private Vector3 targetPosition;
    private bool hasDived = false;
    private Vector3 diveDirection;

    protected override void Start()
    {
        base.Start();
        maxHealth = 40;
        currentHealth = maxHealth;
        scoreValue = 150;
        moveSpeed = 4f;
        fireRate = 2f;

        // Find player and set target
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            targetPosition = player.transform.position;
            diveDirection = (targetPosition - transform.position).normalized;
        }
        else
        {
            diveDirection = Vector3.down;
        }
    }

    protected override void Move()
    {
        if (!hasDived)
        {
            // Move slowly first
            transform.Translate(Vector3.down * (moveSpeed * 0.5f) * Time.deltaTime, Space.World);

            // Start diving when close enough
            if (transform.position.y <= targetPosition.y + 4f)
            {
                hasDived = true;
                // Re-calculate dive direction
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null)
                {
                    diveDirection = (player.transform.position - transform.position).normalized;
                }
            }
        }
        else
        {
            transform.Translate(diveDirection * moveSpeed * 1.5f * Time.deltaTime, Space.World);
        }
    }
}
