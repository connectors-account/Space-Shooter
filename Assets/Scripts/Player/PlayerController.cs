using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Player ship movement. Reads WASD + arrow keys, moves within the visible
    /// screen bounds and cooperates with the pause state. All movement flows
    /// through the deterministic <see cref="Move"/> method so it is unit-testable.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Tooltip("Movement speed in world units per second.")]
        public float moveSpeed = 8f;

        [Header("Screen bounds (world units)")]
        [SerializeField] private float boundXMin = -8f;
        [SerializeField] private float boundXMax = 8f;
        [SerializeField] private float boundYMin = -4.5f;
        [SerializeField] private float boundYMax = 4.5f;

        [Tooltip("Padding kept between the ship and the screen edge.")]
        [SerializeField] private float edgePadding = 0.5f;

        private bool _paused;
        private PlayerShooter _shooter;

        private void Awake()
        {
            _shooter = GetComponent<PlayerShooter>();
            RecalculateBoundsFromCamera();
        }

        private void Update()
        {
            // Respect the global pause state if a GameManager is present.
            if (GameManager.Instance != null)
            {
                _paused = GameManager.Instance.IsPaused;
            }

            float h = Input.GetAxisRaw("Horizontal"); // A/D + Left/Right arrows
            float v = Input.GetAxisRaw("Vertical");   // W/S + Up/Down arrows
            Move(new Vector2(h, v), Time.deltaTime);
        }

        /// <summary>
        /// Moves the ship by <paramref name="input"/> * speed * deltaTime and clamps
        /// the result inside the screen bounds. Does nothing while paused.
        /// </summary>
        public void Move(Vector2 input, float deltaTime)
        {
            if (_paused) return;

            Vector2 dir = Vector2.ClampMagnitude(input, 1f);
            Vector3 pos = transform.position;
            pos.x += dir.x * moveSpeed * deltaTime;
            pos.y += dir.y * moveSpeed * deltaTime;

            pos.x = Mathf.Clamp(pos.x, boundXMin, boundXMax);
            pos.y = Mathf.Clamp(pos.y, boundYMin, boundYMax);
            transform.position = pos;
        }

        /// <summary>Explicitly sets the movement bounds (used by tests and custom cameras).</summary>
        public void SetBounds(float xMin, float xMax, float yMin, float yMax)
        {
            boundXMin = xMin;
            boundXMax = xMax;
            boundYMin = yMin;
            boundYMax = yMax;
        }

        /// <summary>Pauses or resumes local movement.</summary>
        public void SetPaused(bool paused) => _paused = paused;

        /// <summary>Recomputes bounds from the main camera's viewport, if available.</summary>
        public void RecalculateBoundsFromCamera()
        {
            Camera cam = Camera.main;
            if (cam == null || !cam.orthographic) return;

            Vector3 min = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
            Vector3 max = cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));
            boundXMin = min.x + edgePadding;
            boundXMax = max.x - edgePadding;
            boundYMin = min.y + edgePadding;
            boundYMax = max.y - edgePadding;
        }
    }
}
