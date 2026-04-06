using UnityEngine;

/// <summary>
/// Enemy that moves downward in a zigzag pattern.
/// Uses a sine wave for horizontal oscillation while moving down.
/// </summary>
public class EnemyZigZag : EnemyBase
{
    [Header("ZigZag Settings")]
    public float zigzagAmplitude = 3f;
    public float zigzagFrequency = 2f;

    private float startX;
    private float elapsedTime;

    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool();
        startX = transform.position.x;
        elapsedTime = 0f;
    }

    protected override void Move()
    {
        elapsedTime += Time.deltaTime;
        float newX = startX + Mathf.Sin(elapsedTime * zigzagFrequency) * zigzagAmplitude;
        float newY = transform.position.y - moveSpeed * Time.deltaTime;
        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}
