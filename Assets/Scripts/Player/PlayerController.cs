// ============================================================================
// PlayerController.cs — Handles player ship movement
// Reads WASD / Arrow key input and moves the ship within screen bounds.
// ============================================================================
using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 10f;

        private Rigidbody2D _rb;
        private Vector2 _input;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }

        private void Update()
        {
            // Both WASD and Arrow keys feed into GetAxis
            _input.x = Input.GetAxisRaw("Horizontal");
            _input.y = Input.GetAxisRaw("Vertical");
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                _rb.velocity = Vector2.zero;
                return;
            }

            // Frame-independent movement via physics
            _rb.velocity = _input.normalized * moveSpeed;

            // Clamp to screen bounds
            if (GameBounds.Instance != null)
            {
                Vector2 clamped = GameBounds.Instance.ClampToScreen(_rb.position);
                _rb.position = clamped;
            }
        }
    }
}
