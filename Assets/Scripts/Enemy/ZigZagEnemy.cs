using UnityEngine;

/// <summary>
/// Enemy that moves in a zigzag pattern.
/// More challenging to hit than basic enemies.
/// </summary>
public class ZigZagEnemy : EnemyBase
{
    [Header("ZigZag Settings")]
    [Tooltip("Horizontal movement amplitude")]
    public float zigzagAmplitude = 3f;
    
    [Tooltip("Zigzag frequency")]
    public float zigzagFrequency = 2f;
    
    private float startX;
    private float timeOffset;

    protected override void Start()
    {
        base.Start();
        
        // ZigZag enemy settings
        health = 30;
        scoreValue = 150;
        moveSpeed = 2.5f;
        canShoot = true;
        fireRate = 2.5f;
        
        // Store starting X position for zigzag calculation
        startX = transform.position.x;
        
        // Random time offset so enemies don't move in sync
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    protected override void Move()
    {
        // Move down
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;
        
        // Calculate zigzag X position using sine wave
        float newX = startX + Mathf.Sin((Time.time + timeOffset) * zigzagFrequency) * zigzagAmplitude;
        
        // Apply new position
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
}
