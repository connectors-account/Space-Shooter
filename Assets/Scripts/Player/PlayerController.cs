// ============================================================================
// PlayerController.cs - Handles player ship movement via keyboard/mouse
// ============================================================================
using UnityEngine;

/// <summary>
/// Moves the player ship based on WASD / Arrow-key input and clamps to screen bounds.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in world units per second.")]
    [SerializeField] private float moveSpeed = 10f;
    [Tooltip("How fast the ship accelerates to target speed (higher = snappier).")]
    [SerializeField] private float acceleration = 50f;
    [Tooltip("Edge padding so the ship doesn't go fully off-screen.")]
    [SerializeField] private float boundsPadding = 0.5f;

    [Header("Visual Tilt")]
    [Tooltip("Maximum Z-rotation degrees when moving left/right.")]
    [SerializeField] private float tiltAngle = 25f;
    [Tooltip("How fast the tilt lerps.")]
    [SerializeField] private float tiltSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 inputDirection;
    private Vector2 currentVelocity;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        // Gather input every frame for responsiveness.
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            inputDirection = Vector2.zero;
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector2(h, v).normalized;

        // Apply visual tilt based on horizontal input.
        float targetZ = -h * tiltAngle;
        float currentZ = transform.eulerAngles.z;
        if (currentZ > 180f) currentZ -= 360f; // Normalize to -180..180 range.
        float newZ = Mathf.Lerp(currentZ, targetZ, Time.deltaTime * tiltSpeed);
        transform.rotation = Quaternion.Euler(0f, 0f, newZ);
    }

    private void FixedUpdate()
    {
        // Smooth acceleration toward the desired velocity.
        Vector2 targetVelocity = inputDirection * moveSpeed;
        currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = currentVelocity;

        // Clamp position to screen bounds.
        if (GameBounds.Instance != null)
        {
            Vector3 clampedPos = GameBounds.Instance.ClampToScreen(transform.position, boundsPadding);
            // Only reposition if clamping actually changed the position.
            if ((Vector2)clampedPos != (Vector2)transform.position)
            {
                transform.position = new Vector3(clampedPos.x, clampedPos.y, transform.position.z);
                // Zero out velocity on the clamped axes to prevent jitter.
                if (Mathf.Approximately(clampedPos.x, GameBounds.Instance.Min.x + boundsPadding) ||
                    Mathf.Approximately(clampedPos.x, GameBounds.Instance.Max.x - boundsPadding))
                {
                    currentVelocity.x = 0f;
                }
                if (Mathf.Approximately(clampedPos.y, GameBounds.Instance.Min.y + boundsPadding) ||
                    Mathf.Approximately(clampedPos.y, GameBounds.Instance.Max.y - boundsPadding))
                {
                    currentVelocity.y = 0f;
                }
            }
        }
    }
}
