using UnityEngine;

/// <summary>
/// Type 1: Simple drone enemy that moves straight down.
/// Low health, low score, fast and numerous.
/// </summary>
public class EnemyDrone : EnemyBase
{
    [Header("Drone Settings")]
    [SerializeField] private float sineAmplitude = 1.5f;
    [SerializeField] private float sineFrequency = 2f;
    [SerializeField] private bool useSineMovement = true;

    private float spawnX;
    private float timeAlive;

    protected override void Start()
    {
        base.Start();
        maxHealth = 1;
        currentHealth = 1;
        scoreValue = 50;
        moveSpeed = 3.5f;
        fireRate = 3f;
        powerUpDropChance = 0.1f;
        spawnX = transform.position.x;
        timeAlive = 0f;
    }

    /// <summary>
    /// Moves downward with optional sine wave horizontal drift.
    /// </summary>
    protected override void Move()
    {
        timeAlive += Time.deltaTime;

        float yMovement = -moveSpeed * Time.deltaTime;
        float xMovement = 0f;

        if (useSineMovement)
            xMovement = Mathf.Sin(timeAlive * sineFrequency) * sineAmplitude * Time.deltaTime;

        transform.Translate(new Vector3(xMovement, yMovement, 0), Space.World);
    }
}
