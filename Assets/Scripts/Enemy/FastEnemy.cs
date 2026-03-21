using UnityEngine;

/// <summary>
/// Fast enemy: moves quickly in a zigzag pattern, harder to hit.
/// </summary>
public class FastEnemy : EnemyBase
{
    [Header("Fast Enemy")]
    public float zigzagAmplitude = 3f;
    public float zigzagFrequency = 2f;

    private float startX;
    private float timeOffset;

    protected override void Start()
    {
        base.Start();
        maxHealth = 30;
        scoreValue = 150;
        moveSpeed = 4f;
        currentHealth = maxHealth;
        startX = transform.position.x;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    protected override void Move()
    {
        // Move down while zigzagging horizontally
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
        float newX = startX + Mathf.Sin((Time.time + timeOffset) * zigzagFrequency) * zigzagAmplitude;
        transform.position = new Vector3(newX, transform.position.y, 0f);
    }
}
