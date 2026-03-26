// ============================================================================
// PlayerController.cs — Handles player ship movement and input
// ============================================================================
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float smoothTime = 0.05f;

    [Header("Screen Bounds")]
    [SerializeField] private float xMin = -4.2f;
    [SerializeField] private float xMax = 4.2f;
    [SerializeField] private float yMin = -4.5f;
    [SerializeField] private float yMax = 4.5f;

    [Header("Visual Feedback")]
    [SerializeField] private float tiltAmount = 15f;
    [SerializeField] private float tiltSpeed = 5f;

    // Components
    private Rigidbody2D rb;
    private PlayerShooting shooting;
    private SpriteRenderer spriteRenderer;

    // Runtime
    private Vector2 moveInput;
    private Vector2 velocity;
    private Vector2 currentVelocity;
    private bool isControllable = true;

    // =========================================================================
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        shooting = GetComponent<PlayerShooting>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (!isControllable) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

        GatherInput();
        HandleTilt();
    }

    private void FixedUpdate()
    {
        if (!isControllable) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

        Move();
        ClampPosition();
    }

    // =========================================================================
    // Input
    // =========================================================================
    private void GatherInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Also support WASD explicitly
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v = -1f;

        moveInput = new Vector2(h, v).normalized;
    }

    // =========================================================================
    // Movement
    // =========================================================================
    private void Move()
    {
        Vector2 targetVelocity = moveInput * moveSpeed;
        velocity = Vector2.SmoothDamp(velocity, targetVelocity, ref currentVelocity, smoothTime);
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void ClampPosition()
    {
        Vector2 pos = rb.position;
        pos.x = Mathf.Clamp(pos.x, xMin, xMax);
        pos.y = Mathf.Clamp(pos.y, yMin, yMax);
        rb.position = pos;
    }

    // =========================================================================
    // Visual Tilt (ship banks when moving left/right)
    // =========================================================================
    private void HandleTilt()
    {
        float targetZ = -moveInput.x * tiltAmount;
        float currentZ = transform.eulerAngles.z;
        if (currentZ > 180f) currentZ -= 360f;
        float newZ = Mathf.Lerp(currentZ, targetZ, Time.deltaTime * tiltSpeed);
        transform.rotation = Quaternion.Euler(0f, 0f, newZ);
    }

    // =========================================================================
    // Public API
    // =========================================================================
    public void SetControllable(bool controllable)
    {
        isControllable = controllable;
        if (!controllable)
        {
            velocity = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        moveSpeed = 8f * multiplier;
    }

    public Vector2 Velocity => velocity;
    public bool IsControllable => isControllable;
}
