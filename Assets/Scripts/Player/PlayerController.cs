using UnityEngine;

/// <summary>
/// Controls player ship movement within game boundaries.
/// Attach this script to the Player GameObject.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Player movement speed in units per second")]
    [SerializeField] private float moveSpeed = 10f;
    
    [Header("Boundary Settings")]
    [Tooltip("Horizontal boundary limit (positive and negative)")]
    [SerializeField] private float horizontalBoundary = 8f;
    
    [Tooltip("Vertical boundary limit (positive and negative)")]
    [SerializeField] private float verticalBoundary = 4.5f;
    
    // Cached component references
    private Rigidbody2D rb;
    
    // Input values
    private float horizontalInput;
    private float verticalInput;
    
    /// <summary>
    /// Initialize component references on awake.
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // If no Rigidbody2D exists, add one
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
    
    /// <summary>
    /// Gather input every frame.
    /// </summary>
    private void Update()
    {
        // Don't process input if game is paused or over
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
        {
            horizontalInput = 0f;
            verticalInput = 0f;
            return;
        }
        
        // Get movement input from keyboard or controller
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }
    
    /// <summary>
    /// Apply physics-based movement in FixedUpdate.
    /// </summary>
    private void FixedUpdate()
    {
        // Calculate movement direction
        Vector2 movement = new Vector2(horizontalInput, verticalInput).normalized;
        
        // Apply movement
        rb.velocity = movement * moveSpeed;
        
        // Clamp position within boundaries
        ClampPosition();
    }
    
    /// <summary>
    /// Keeps the player within the defined game boundaries.
    /// </summary>
    private void ClampPosition()
    {
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -horizontalBoundary, horizontalBoundary);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -verticalBoundary, verticalBoundary);
        transform.position = clampedPosition;
    }
    
    /// <summary>
    /// Sets the movement speed (useful for power-ups).
    /// </summary>
    /// <param name="newSpeed">New movement speed value</param>
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
    
    /// <summary>
    /// Gets the current movement speed.
    /// </summary>
    /// <returns>Current movement speed</returns>
    public float GetMoveSpeed()
    {
        return moveSpeed;
    }
}
