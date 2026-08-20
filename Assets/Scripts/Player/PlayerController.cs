using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Utilities;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Player ship movement using the legacy Input system (WASD + arrows).
    /// Smooth accel/decel, clamped to screen bounds, banking tilt on horizontal move,
    /// and an invincibility flash driven by PlayerHealth. Also toggles a shield child object.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float maxSpeed = 9f;
        public float acceleration = 60f;
        public float deceleration = 40f;
        public float tiltAmount = 25f;      // degrees of bank at full horizontal speed
        public float tiltSpeed = 8f;

        [Header("Bounds Padding")]
        public float padX = 0.5f;
        public float padY = 0.5f;

        [Header("Shield Visual")]
        public GameObject shieldObject;      // child, toggled by shield power-up / state

        private Vector2 _velocity;
        private SpriteRenderer _sr;
        private PlayerHealth _health;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _health = GetComponent<PlayerHealth>();

            // Ensure a kinematic Rigidbody2D with full contacts so triggers fire against
            // kinematic enemy bullets/enemies and static power-up colliders.
            var rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;

            // Generate the ship sprite at runtime if the prefab has none assigned.
            if (_sr != null && _sr.sprite == null)
                _sr.sprite = SpriteGenerator.CreateShip(new Color(0.3f, 0.85f, 1f), Color.white);

            EnsureShieldVisual();
        }

        private void EnsureShieldVisual()
        {
            if (shieldObject != null) { shieldObject.SetActive(false); return; }

            // Build a translucent blue shield ring as a child if one wasn't wired up.
            var go = new GameObject("Shield");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one * 1.4f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateCircle(24, new Color(0.3f, 0.6f, 1f, 0.35f));
            sr.sortingOrder = 5;
            shieldObject = go;
            shieldObject.SetActive(false);
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                _velocity = Vector2.MoveTowards(_velocity, Vector2.zero, deceleration * Time.deltaTime);
                return;
            }

            HandleMovement();
            HandleTilt();
            HandleInvincibilityFlash();
        }

        private void HandleMovement()
        {
            float ix = Input.GetAxisRaw("Horizontal"); // A/D + arrows
            float iy = Input.GetAxisRaw("Vertical");   // W/S + arrows
            Vector2 input = new Vector2(ix, iy);
            if (input.sqrMagnitude > 1f) input.Normalize();

            Vector2 targetVel = input * maxSpeed;

            // Accelerate toward target, decelerate when no input on that axis.
            _velocity.x = Approach(_velocity.x, targetVel.x, input.x != 0 ? acceleration : deceleration);
            _velocity.y = Approach(_velocity.y, targetVel.y, input.y != 0 ? acceleration : deceleration);

            Vector3 next = transform.position + (Vector3)(_velocity * Time.deltaTime);
            if (ScreenBounds.Instance != null)
                next = ScreenBounds.Instance.Clamp(next, padX, padY);
            transform.position = next;
        }

        private float Approach(float current, float target, float rate)
        {
            return Mathf.MoveTowards(current, target, rate * Time.deltaTime);
        }

        private void HandleTilt()
        {
            float targetZ = -(_velocity.x / Mathf.Max(0.01f, maxSpeed)) * tiltAmount;
            Quaternion targetRot = Quaternion.Euler(0, 0, targetZ);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, tiltSpeed * Time.deltaTime);
        }

        private void HandleInvincibilityFlash()
        {
            if (_sr == null) return;
            if (_health != null && _health.IsInvincible)
            {
                // Blink alpha while invincible.
                float a = Mathf.PingPong(Time.time * 8f, 1f);
                var c = _sr.color;
                c.a = Mathf.Lerp(0.25f, 1f, a);
                _sr.color = c;
            }
            else
            {
                var c = _sr.color;
                c.a = 1f;
                _sr.color = c;
            }
        }

        public void SetShieldVisual(bool active)
        {
            if (shieldObject != null) shieldObject.SetActive(active);
        }
    }
}
