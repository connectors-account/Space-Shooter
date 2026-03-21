using UnityEngine;

/// <summary>
/// Bomber enemy: flies across the screen horizontally, drops bombs downward.
/// </summary>
public class BomberEnemy : EnemyBase
{
    [Header("Bomber Enemy")]
    public GameObject bombPrefab;
    public float bombInterval = 1.5f;
    public float horizontalSpeed = 3f;

    private float nextBombTime;
    private int moveDir;

    protected override void Start()
    {
        base.Start();
        maxHealth = 80;
        scoreValue = 250;
        moveSpeed = 1f; // slow vertical descent
        currentHealth = maxHealth;

        // Random horizontal direction
        moveDir = Random.value > 0.5f ? 1 : -1;
        nextBombTime = Time.time + Random.Range(0.5f, bombInterval);
    }

    protected override void Move()
    {
        // Mostly horizontal movement with slow descent
        Vector3 movement = new Vector3(moveDir * horizontalSpeed, -moveSpeed, 0f);
        transform.Translate(movement * Time.deltaTime, Space.World);

        // Bounce off screen edges
        if (transform.position.x > 8.5f || transform.position.x < -8.5f)
        {
            moveDir *= -1;
        }
    }

    protected override void Attack()
    {
        if (Time.time < nextBombTime || bombPrefab == null) return;
        nextBombTime = Time.time + bombInterval;

        Instantiate(bombPrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
        AudioManager.Instance?.PlaySFX("EnemyShoot");
    }
}
