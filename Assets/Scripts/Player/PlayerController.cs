using UnityEngine;

/// <summary>
/// Handles player ship movement using keyboard input (Arrow keys or WASD).
/// Clamps position within game bounds.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float smoothing = 0.1f;

    private Vector2 velocity;
    private Vector2 currentVelocity;

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 targetVelocity = new Vector2(horizontal, vertical).normalized * moveSpeed;
        velocity = Vector2.SmoothDamp(velocity, targetVelocity, ref currentVelocity, smoothing);

        Vector3 newPosition = transform.position + (Vector3)velocity * Time.deltaTime;

        if (GameBounds.Instance != null)
        {
            newPosition = GameBounds.Instance.ClampPosition(newPosition);
        }

        transform.position = newPosition;

        // Slight tilt based on horizontal movement for visual feedback
        float tilt = -velocity.x * 2f;
        transform.rotation = Quaternion.Euler(0f, 0f, tilt);
    }
}
