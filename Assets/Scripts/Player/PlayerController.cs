using System.Collections;
using UnityEngine;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Handles player movement, screen clamping, tilt, invincibility flashing and speed power-ups.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float smoothing = 12f;
        [SerializeField] private float tiltAngle = 15f;

        [Header("Bounds Padding")]
        [SerializeField] private float horizontalPadding = 0.4f;
        [SerializeField] private float verticalPadding = 0.4f;

        [Header("Invincibility")]
        [SerializeField] private float flashInterval = 0.1f;

        private Camera cam;
        private SpriteRenderer sr;
        private TrailRenderer trail;
        private Vector2 inputVector;
        private float currentSpeed;
        private float speedMultiplier = 1f;
        private Coroutine speedBoostRoutine;
        private Coroutine flashRoutine;
        private bool controlsEnabled = true;

        private void Awake()
        {
            cam = Camera.main;
            sr = GetComponent<SpriteRenderer>();
            trail = GetComponent<TrailRenderer>();
            currentSpeed = moveSpeed;
        }

        private void OnEnable()
        {
            controlsEnabled = true;
        }

        private void Update()
        {
            if (!controlsEnabled)
            {
                inputVector = Vector2.zero;
                return;
            }

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            inputVector = new Vector2(h, v).normalized;
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        private void MovePlayer()
        {
            Vector3 delta = (Vector3)inputVector * currentSpeed * speedMultiplier * Time.fixedDeltaTime;
            Vector3 target = transform.position + delta;
            target = ClampToScreen(target);
            transform.position = Vector3.Lerp(transform.position, target, smoothing * Time.fixedDeltaTime);

            // Tilt based on horizontal input.
            float targetTilt = -inputVector.x * tiltAngle;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetTilt);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, smoothing * Time.fixedDeltaTime);
        }

        private Vector3 ClampToScreen(Vector3 position)
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return position;

            Vector3 min = cam.ViewportToWorldPoint(new Vector3(0f, 0f, Mathf.Abs(cam.transform.position.z)));
            Vector3 max = cam.ViewportToWorldPoint(new Vector3(1f, 1f, Mathf.Abs(cam.transform.position.z)));

            position.x = Mathf.Clamp(position.x, min.x + horizontalPadding, max.x - horizontalPadding);
            position.y = Mathf.Clamp(position.y, min.y + verticalPadding, max.y - verticalPadding);
            position.z = 0f;
            return position;
        }

        // ---------------- Power-up: speed boost ----------------

        public void ApplySpeedBoost(float multiplier, float duration)
        {
            if (speedBoostRoutine != null) StopCoroutine(speedBoostRoutine);
            speedBoostRoutine = StartCoroutine(SpeedBoostRoutine(multiplier, duration));
        }

        private IEnumerator SpeedBoostRoutine(float multiplier, float duration)
        {
            speedMultiplier = multiplier;
            yield return new WaitForSeconds(duration);
            speedMultiplier = 1f;
            speedBoostRoutine = null;
        }

        // ---------------- Invincibility flashing ----------------

        public void StartInvincibilityFlash(float duration)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine(duration));
        }

        private IEnumerator FlashRoutine(float duration)
        {
            float elapsed = 0f;
            bool visible = true;
            Color baseColor = sr.color;
            while (elapsed < duration)
            {
                visible = !visible;
                Color c = baseColor;
                c.a = visible ? 1f : 0.3f;
                sr.color = c;
                yield return new WaitForSeconds(flashInterval);
                elapsed += flashInterval;
            }
            Color restore = baseColor;
            restore.a = 1f;
            sr.color = restore;
            flashRoutine = null;
        }

        public void SetControlsEnabled(bool enabled)
        {
            controlsEnabled = enabled;
        }

        public void ResetToCenter()
        {
            if (cam == null) cam = Camera.main;
            Vector3 pos = transform.position;
            pos.x = 0f;
            if (cam != null)
            {
                Vector3 min = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.15f, Mathf.Abs(cam.transform.position.z)));
                pos.y = min.y;
            }
            pos.z = 0f;
            transform.position = pos;
            transform.rotation = Quaternion.identity;
            if (trail != null) trail.Clear();
        }
    }
}
