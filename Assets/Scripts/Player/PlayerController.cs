using UnityEngine;

/// <summary>
/// Handles player ship movement via keyboard (WASD/Arrows) and mouse input.
/// Clamps position within game bounds.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private bool useMouseControl = false;

    [Header("Visual Feedback")]
    [SerializeField] private float tiltAngle = 15f; // Tilt ship when strafing

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Camera mainCamera;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        GatherInput();
        ApplyVisualTilt();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        Move();
        ClampPosition();
    }

    private void GatherInput()
    {
        if (useMouseControl)
        {
            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            Vector2 direction = ((Vector2)mouseWorld - rb.position);
            moveInput = direction.magnitude > 0.1f ? direction.normalized : Vector2.zero;

            // Snap to mouse if close enough
            if (direction.magnitude < 0.2f)
                moveInput = Vector2.zero;
        }
        else
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(h, v).normalized;
        }
    }

    private void Move()
    {
        rb.velocity = moveInput * moveSpeed;
    }

    private void ClampPosition()
    {
        if (GameManager.Instance == null) return;

        float clampedX = Mathf.Clamp(rb.position.x,
            -GameManager.Instance.gameBoundsX + 0.5f,
             GameManager.Instance.gameBoundsX - 0.5f);
        float clampedY = Mathf.Clamp(rb.position.y,
            -GameManager.Instance.gameBoundsY + 0.5f,
             GameManager.Instance.gameBoundsY - 0.5f);

        rb.position = new Vector2(clampedX, clampedY);
    }

    private void ApplyVisualTilt()
    {
        float targetZ = -moveInput.x * tiltAngle;
        float currentZ = transform.eulerAngles.z;
        if (currentZ > 180f) currentZ -= 360f;
        float smoothZ = Mathf.Lerp(currentZ, targetZ, Time.deltaTime * 10f);
        transform.rotation = Quaternion.Euler(0f, 0f, smoothZ);
    }

    public void SetMouseControl(bool enabled)
    {
        useMouseControl = enabled;
    }
}
