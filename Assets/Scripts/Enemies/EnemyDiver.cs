// ============================================================================
// EnemyDiver.cs - Fast enemy that dives toward the player's position
// High speed, low health, kamikaze-style threat.
// ============================================================================
using UnityEngine;

/// <summary>
/// Locks onto the player's position at spawn, then dives straight toward that
/// point at high speed. Does not change course mid-flight.
/// </summary>
public class EnemyDiver : EnemyBase
{
    [Header("Diver Settings")]
    [Tooltip("Speed multiplier for the dive (on top of base moveSpeed).")]
    [SerializeField] private float diveSpeedMultiplier = 2f;

    private Vector3 targetPosition;
    private Vector3 diveDirection;
    private bool hasLockedTarget;

    protected override void OnEnable()
    {
        base.OnEnable();
        hasLockedTarget = false;
    }

    protected override void MovementPattern()
    {
        // Lock onto the player's current position once, then dive in that direction.
        if (!hasLockedTarget)
        {
            if (playerTransform != null)
            {
                targetPosition = playerTransform.position;
            }
            else
            {
                // No player found; just dive downward.
                targetPosition = transform.position + Vector3.down * 20f;
            }
            diveDirection = (targetPosition - transform.position).normalized;
            hasLockedTarget = true;

            // Rotate to face the dive direction.
            float angle = Mathf.Atan2(diveDirection.y, diveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }

        // Move along the locked dive direction.
        float speed = moveSpeed * diveSpeedMultiplier;
        transform.Translate(diveDirection * speed * Time.deltaTime, Space.World);
    }
}
