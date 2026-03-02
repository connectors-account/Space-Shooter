using UnityEngine;

/// <summary>
/// Enemy that dives toward the player's position.
/// Aggressive enemy type that requires quick reflexes to avoid.
/// </summary>
public class DiveBomberEnemy : EnemyBase
{
    [Header("Dive Bomber Settings")]
    [Tooltip("Time to wait before diving")]
    public float diveDelay = 1f;
    
    [Tooltip("Speed multiplier during dive")]
    public float diveSpeedMultiplier = 2f;
    
    private enum State { Entering, Hovering, Diving }
    private State currentState = State.Entering;
    
    private float hoverTimer = 0f;
    private Vector3 targetPosition;
    private float hoverY = 3f;
    private Transform playerTransform;

    protected override void Start()
    {
        base.Start();
        
        // Dive bomber settings
        health = 40;
        scoreValue = 200;
        moveSpeed = 4f;
        canShoot = true;
        fireRate = 3f;
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    protected override void Move()
    {
        switch (currentState)
        {
            case State.Entering:
                // Move down to hover position
                transform.position += Vector3.down * moveSpeed * Time.deltaTime;
                
                if (transform.position.y <= hoverY)
                {
                    currentState = State.Hovering;
                    hoverTimer = 0f;
                }
                break;
                
            case State.Hovering:
                // Hover and track player
                hoverTimer += Time.deltaTime;
                
                if (playerTransform != null)
                {
                    // Slowly move toward player's X position
                    float targetX = playerTransform.position.x;
                    float newX = Mathf.MoveTowards(transform.position.x, targetX, moveSpeed * 0.5f * Time.deltaTime);
                    transform.position = new Vector3(newX, transform.position.y, transform.position.z);
                }
                
                // Start diving after delay
                if (hoverTimer >= diveDelay)
                {
                    // Lock target position
                    if (playerTransform != null)
                    {
                        targetPosition = playerTransform.position;
                    }
                    else
                    {
                        targetPosition = new Vector3(transform.position.x, -6f, 0f);
                    }
                    currentState = State.Diving;
                }
                break;
                
            case State.Diving:
                // Dive toward locked target position
                Vector3 direction = (targetPosition - transform.position).normalized;
                transform.position += direction * moveSpeed * diveSpeedMultiplier * Time.deltaTime;
                break;
        }
    }
}
