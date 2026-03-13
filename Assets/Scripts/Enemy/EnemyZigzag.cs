using UnityEngine;

/// <summary>
/// Enemy that moves in a zigzag pattern while descending.
/// </summary>
public class EnemyZigzag : EnemyBase
{
    [Header("Zigzag Settings")]
    public float zigzagAmplitude = 3f;
    public float zigzagFrequency = 2f;

    private float startX;
    private float timeOffset;

    protected override void Start()
    {
        base.Start();
        maxHealth = 30;
        currentHealth = maxHealth;
        scoreValue = 100;
        moveSpeed = 2.5f;
        fireRate = 2.5f;
        startX = transform.position.x;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    protected override void Move()
    {
        float newX = startX + Mathf.Sin((Time.time + timeOffset) * zigzagFrequency) * zigzagAmplitude;
        float newY = transform.position.y - moveSpeed * Time.deltaTime;
        transform.position = new Vector3(newX, newY, 0f);
    }
}
