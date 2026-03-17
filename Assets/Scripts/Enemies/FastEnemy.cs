using UnityEngine;

/// <summary>
/// Fast enemy: moves quickly in a zigzag pattern, harder to hit.
/// </summary>
public class FastEnemy : EnemyBase
{
    [Header("Zigzag")]
    public float zigzagAmplitude = 3f;
    public float zigzagFrequency = 2f;

    private float startX;
    private float timeOffset;

    protected override void Start()
    {
        maxHealth = 1;
        moveSpeed = 5f;
        scoreValue = 200;
        fireRate = 4f;
        canShoot = false;
        dropChance = 0.15f;

        startX = transform.position.x;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
        base.Start();
    }

    protected override void Move()
    {
        // Move down with zigzag
        float newX = startX + Mathf.Sin((Time.time + timeOffset) * zigzagFrequency) * zigzagAmplitude;
        float newY = transform.position.y - moveSpeed * Time.deltaTime;
        transform.position = new Vector3(newX, newY, 0);
    }
}
