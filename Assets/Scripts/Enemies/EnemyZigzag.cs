// ============================================================================
// EnemyZigzag.cs - Enemy that weaves left/right while descending
// More unpredictable than EnemyStraight; harder to hit.
// ============================================================================
using UnityEngine;

/// <summary>
/// Descends while oscillating horizontally in a sine-wave zigzag pattern.
/// </summary>
public class EnemyZigzag : EnemyBase
{
    [Header("Zigzag Settings")]
    [Tooltip("How far the enemy sways left and right (world units).")]
    [SerializeField] private float zigzagAmplitude = 3f;
    [Tooltip("How fast the enemy oscillates (cycles per second).")]
    [SerializeField] private float zigzagFrequency = 2f;

    private float startX;

    protected override void OnEnable()
    {
        base.OnEnable();
        startX = transform.position.x;
    }

    protected override void MovementPattern()
    {
        float elapsed = Time.time - spawnTime;

        // Horizontal sine oscillation.
        float xOffset = Mathf.Sin(elapsed * zigzagFrequency * Mathf.PI * 2f) * zigzagAmplitude;
        float newX = startX + xOffset;

        // Continuous downward motion.
        float newY = transform.position.y - moveSpeed * Time.deltaTime;

        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}
