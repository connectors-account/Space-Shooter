using UnityEngine;

/// <summary>
/// Enemy that dives toward the player's position, then continues past.
/// Calculates trajectory once on spawn and follows it.
/// </summary>
public class EnemyDiver : EnemyBase
{
    [Header("Diver Settings")]
    public float diveSpeed = 5f;

    private Vector3 diveDirection;
    private bool hasTarget;

    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool();
        hasTarget = false;

        // Lock onto player position at spawn time
        if (playerTransform != null)
        {
            diveDirection = (playerTransform.position - transform.position).normalized;
            hasTarget = true;
        }
        else
        {
            diveDirection = Vector3.down;
        }
    }

    protected override void Move()
    {
        float speed = hasTarget ? diveSpeed : moveSpeed;
        transform.Translate(diveDirection * speed * Time.deltaTime, Space.World);

        // Rotate to face movement direction
        float angle = Mathf.Atan2(diveDirection.y, diveDirection.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
