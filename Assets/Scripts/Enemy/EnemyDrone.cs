using UnityEngine;

/// <summary>
/// Basic drone enemy: flies straight down, occasionally shoots.
/// </summary>
public class EnemyDrone : EnemyBase
{
    [Header("Drone Settings")]
    [SerializeField] private float wobbleAmplitude = 0.5f;
    [SerializeField] private float wobbleFrequency = 2f;

    private float startX;
    private float spawnTime;

    protected override void Start()
    {
        base.Start();
        maxHealth = 20;
        scoreValue = 100;
        moveSpeed = 3f;
        fireRate = 3f;
        contactDamage = 15;
        currentHealth = maxHealth;
        startX = transform.position.x;
        spawnTime = Time.time;
    }

    protected override void Move()
    {
        float elapsed = Time.time - spawnTime;
        float wobbleX = startX + Mathf.Sin(elapsed * wobbleFrequency) * wobbleAmplitude;
        float newY = transform.position.y - moveSpeed * Time.deltaTime;
        transform.position = new Vector3(wobbleX, newY, transform.position.z);
    }
}
