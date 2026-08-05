using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Drives player movement via Rigidbody2D, clamped to the visible screen
    /// bounds. Reads input from <see cref="PlayerInputHandler"/>, handles the
    /// speed-boost power-up multiplier, and manages invincibility frames with
    /// a flashing sprite after taking a hit. Screen-wrap is intentionally
    /// disabled – the player is clamped only.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float screenPadding = 0.4f;

        [Header("Invincibility")]
        [SerializeField] private float invincibilityDuration = 1.5f;
        [SerializeField] private float flashInterval = 0.1f;

        public bool IsInvincible { get; private set; }

        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private PlayerInputHandler _input;
        private Camera _camera;

        private float _speedMultiplier = 1f;
        private float _speedBoostTimer;

        private float _invincibleTimer;
        private float _flashTimer;

        private Vector2 _minBounds;
        private Vector2 _maxBounds;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            _input = GetComponent<PlayerInputHandler>();
            if (_input == null) _input = gameObject.AddComponent<PlayerInputHandler>();

            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (_sr.sprite == null)
                _sr.sprite = Utilities.SpriteGenerator.CreatePlayerSprite();
        }

        private void Start()
        {
            _camera = Camera.main;
            if (_camera == null) _camera = FindObjectOfType<Camera>();
            ComputeBounds();
        }

        private void ComputeBounds()
        {
            if (_camera == null || !_camera.orthographic)
            {
                _minBounds = new Vector2(-3f, -5f);
                _maxBounds = new Vector2(3f, 5f);
                return;
            }

            float halfH = _camera.orthographicSize;
            float halfW = halfH * _camera.aspect;
            Vector3 cam = _camera.transform.position;

            // Sprite extents so the ship never clips off-screen.
            float extX = _sr.sprite != null ? _sr.sprite.bounds.extents.x : 0.5f;
            float extY = _sr.sprite != null ? _sr.sprite.bounds.extents.y : 0.5f;

            _minBounds = new Vector2(cam.x - halfW + extX + screenPadding, cam.y - halfH + extY + screenPadding);
            _maxBounds = new Vector2(cam.x + halfW - extX - screenPadding, cam.y + halfH - extY - screenPadding);
        }

        private void Update()
        {
            // Tick down power-up and invincibility timers on the frame clock.
            if (_speedBoostTimer > 0f)
            {
                _speedBoostTimer -= Time.deltaTime;
                if (_speedBoostTimer <= 0f)
                    _speedMultiplier = 1f;
            }

            if (IsInvincible)
            {
                _invincibleTimer -= Time.deltaTime;
                _flashTimer -= Time.deltaTime;
                if (_flashTimer <= 0f)
                {
                    _flashTimer = flashInterval;
                    _sr.enabled = !_sr.enabled;
                }
                if (_invincibleTimer <= 0f)
                    EndInvincibility();
            }
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                _rb.velocity = Vector2.zero;
                return;
            }

            Vector2 move = _input != null ? _input.MoveInput : Vector2.zero;
            if (move.sqrMagnitude > 1f) move.Normalize();

            Vector2 target = _rb.position + move * (moveSpeed * _speedMultiplier * Time.fixedDeltaTime);
            target.x = Mathf.Clamp(target.x, _minBounds.x, _maxBounds.x);
            target.y = Mathf.Clamp(target.y, _minBounds.y, _maxBounds.y);
            _rb.MovePosition(target);
        }

        // -----------------------------------------------------------------
        // Speed boost power-up
        // -----------------------------------------------------------------
        public void ActivateSpeedBoost(float multiplier, float duration)
        {
            _speedMultiplier = Mathf.Max(1f, multiplier);
            _speedBoostTimer = duration;
        }

        // -----------------------------------------------------------------
        // Invincibility frames
        // -----------------------------------------------------------------
        public void BeginInvincibility()
        {
            IsInvincible = true;
            _invincibleTimer = invincibilityDuration;
            _flashTimer = flashInterval;
        }

        public void BeginInvincibility(float duration)
        {
            IsInvincible = true;
            _invincibleTimer = duration;
            _flashTimer = flashInterval;
        }

        private void EndInvincibility()
        {
            IsInvincible = false;
            _sr.enabled = true;
        }

        /// <summary>Reset position/state when (re)spawning.</summary>
        public void ResetPlayer()
        {
            EndInvincibility();
            _speedMultiplier = 1f;
            _speedBoostTimer = 0f;
            _rb.velocity = Vector2.zero;
            if (_camera != null)
            {
                Vector3 cam = _camera.transform.position;
                float halfH = _camera.orthographic ? _camera.orthographicSize : 5f;
                _rb.position = new Vector2(cam.x, cam.y - halfH * 0.6f);
            }
        }
    }
}
