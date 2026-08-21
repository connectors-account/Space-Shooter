using UnityEngine;
using UnityEngine.InputSystem;
using SpaceShooter.Utilities;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Player ship movement using the Unity New Input System.
    /// Supports WASD + arrow keys, smooth acceleration/deceleration, screen clamping,
    /// and a visual tilt on horizontal movement. Base speed 8, modified by power-ups.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float baseSpeed = 8f;
        [SerializeField] private float acceleration = 40f;
        [SerializeField] private float deceleration = 30f;
        [SerializeField] private float edgeMargin = 0.5f;

        [Header("Tilt")]
        [SerializeField] private Transform spriteTransform;
        [SerializeField] private float maxTiltAngle = 25f;
        [SerializeField] private float tiltSpeed = 10f;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private Vector2 _currentVelocity;
        private float _speedMultiplier = 1f;

        private PlayerInputActions _inputActions;

        public float CurrentSpeed => baseSpeed * _speedMultiplier;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.bodyType = RigidbodyType2D.Kinematic;

            if (spriteTransform == null)
            {
                spriteTransform = transform;
            }

            _inputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _inputActions.Enable();
            _inputActions.Gameplay.Move.performed += OnMove;
            _inputActions.Gameplay.Move.canceled += OnMove;
        }

        private void OnDisable()
        {
            _inputActions.Gameplay.Move.performed -= OnMove;
            _inputActions.Gameplay.Move.canceled -= OnMove;
            _inputActions.Disable();
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            _moveInput = ctx.ReadValue<Vector2>();
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = Mathf.Max(0.1f, multiplier);
        }

        private void Update()
        {
            ApplyTilt();
        }

        private void FixedUpdate()
        {
            Vector2 targetVelocity = _moveInput.normalized * CurrentSpeed;

            // Smooth accelerate toward target, decelerate when no input.
            float rate = _moveInput.sqrMagnitude > 0.01f ? acceleration : deceleration;
            _currentVelocity = Vector2.MoveTowards(_currentVelocity, targetVelocity, rate * Time.fixedDeltaTime);

            Vector2 newPos = _rb.position + _currentVelocity * Time.fixedDeltaTime;

            if (ScreenBounds.Instance != null)
            {
                newPos = ScreenBounds.Instance.Clamp(newPos, edgeMargin);
            }

            _rb.MovePosition(newPos);
        }

        private void ApplyTilt()
        {
            float targetTilt = -_moveInput.x * maxTiltAngle;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetTilt);
            spriteTransform.localRotation = Quaternion.Lerp(
                spriteTransform.localRotation, targetRot, tiltSpeed * Time.deltaTime);
        }

        private void OnDestroy()
        {
            _inputActions?.Dispose();
        }
    }
}
