using UnityEngine;

/// <summary>
/// Controls enemy movement behavior.
/// Attach this script to enemy GameObjects.
/// </summary>
public class EnemyController : MonoBehaviour
{
    /// <summary>
    /// Defines different movement patterns for enemies.
    /// </summary>
    public enum MovementPattern
    {
        Straight,       // Move straight down
        Zigzag,         // Move down with horizontal oscillation
        Sine,           // Move in a sine wave pattern
        Homing,         // Move toward the player
        Stationary      // Stay in place (for boss-type enemies)
    }
    
    [Header("Movement Settings")]
    [Tooltip("Speed of enemy movement")]
    [SerializeField] private float moveSpeed = 3f;
    
    [Tooltip("Movement pattern for this enemy")]
    [SerializeField] private MovementPattern movementPattern = MovementPattern.Straight;
    
    [Header("Pattern Settings")]
    [Tooltip("Amplitude for zigzag/sine patterns")]
    [SerializeField] private float patternAmplitude = 2f;
    
    [Tooltip("Frequency for zigzag/sine patterns")]
    [SerializeField] private float patternFrequency = 2f;
    
    [Tooltip("Homing strength (0-1) for homing pattern")]
    [SerializeField] private float homingStrength = 0.5f;
    
    [Header("Boundary Settings")]
    [Tooltip("Y position at which enemy is destroyed")]
    [SerializeField] private float destroyY = -6f;
    
    // Internal state
    private float startX;
    private float timeAlive;
    private Transform playerTransform;
    private Rigidbody2D rb;
    
    /// <summary>
    /// Initialize enemy state.
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
    
    /// <summary>
    /// Cache initial position and find player.
    /// </summary>
    private void Start()
    {
        startX = transform.position.x;
        timeAlive = 0f;
        
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }
    
    /// <summary>
    /// Update movement every frame.
    /// </summary>
    private void Update()
    {
        timeAlive += Time.deltaTime;
        
        // Check for out of bounds
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Apply physics-based movement.
    /// </summary>
    private void FixedUpdate()
    {
        Vector2 movement = CalculateMovement();
        rb.velocity = movement;
    }
    
    /// <summary>
    /// Calculate movement based on the selected pattern.
    /// </summary>
    /// <returns>Movement vector to apply</returns>
    private Vector2 CalculateMovement()
    {
        Vector2 movement = Vector2.zero;
        
        switch (movementPattern)
        {
            case MovementPattern.Straight:
                movement = Vector2.down * moveSpeed;
                break;
                
            case MovementPattern.Zigzag:
                float zigzagX = Mathf.Sign(Mathf.Sin(timeAlive * patternFrequency)) * patternAmplitude;
                movement = new Vector2(zigzagX, -moveSpeed);
                break;
                
            case MovementPattern.Sine:
                float sineX = Mathf.Cos(timeAlive * patternFrequency) * patternAmplitude;
                movement = new Vector2(sineX, -moveSpeed);
                break;
                
            case MovementPattern.Homing:
                if (playerTransform != null)
                {
                    Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
                    Vector2 downward = Vector2.down;
                    movement = Vector2.Lerp(downward, directionToPlayer, homingStrength) * moveSpeed;
                }
                else
                {
                    movement = Vector2.down * moveSpeed;
                }
                break;
                
            case MovementPattern.Stationary:
                movement = Vector2.zero;
                break;
        }
        
        return movement;
    }
    
    /// <summary>
    /// Set the movement speed.
    /// </summary>
    /// <param name="speed">New movement speed</param>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    /// <summary>
    /// Set the movement pattern.
    /// </summary>
    /// <param name="pattern">New movement pattern</param>
    public void SetMovementPattern(MovementPattern pattern)
    {
        movementPattern = pattern;
    }
    
    /// <summary>
    /// Configure pattern parameters.
    /// </summary>
    /// <param name="amplitude">Pattern amplitude</param>
    /// <param name="frequency">Pattern frequency</param>
    public void SetPatternParameters(float amplitude, float frequency)
    {
        patternAmplitude = amplitude;
        patternFrequency = frequency;
    }
}
