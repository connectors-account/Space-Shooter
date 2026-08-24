using System.Collections;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Utilities;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SpaceShooter.Player
{
    /// <summary>
    /// Handles player ship movement, screen clamping, pause handling and speed boosts.
    /// Supports both the legacy Input Manager and the new Input System via #if directives.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = Constants.PlayerMoveSpeed;
        [SerializeField] private float screenPadding = Constants.ScreenPadding;

        private Camera _camera;
        private float _baseMoveSpeed;
        private float _currentSpeedMultiplier = 1f;
        private Coroutine _speedBoostRoutine;

        public float CurrentSpeed => moveSpeed * _currentSpeedMultiplier;

        private void Awake()
        {
            _camera = Camera.main;
            _baseMoveSpeed = moveSpeed;
        }

        private void Update()
        {
            // Freeze movement while paused, in menu or game over.
            if (GameManager.HasInstance && GameManager.Instance.State != GameState.Playing)
            {
                return;
            }

            Vector2 input = ReadMovementInput();
            Vector3 delta = new Vector3(input.x, input.y, 0f) * (CurrentSpeed * Time.deltaTime);
            Vector3 target = transform.position + delta;

            transform.position = ClampToScreen(target);
        }

        private Vector2 ReadMovementInput()
        {
            float h;
            float v;

#if ENABLE_INPUT_SYSTEM
            // New Input System path.
            Keyboard keyboard = Keyboard.current;
            h = 0f;
            v = 0f;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) h -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) h += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) v -= 1f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) v += 1f;
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                if (Mathf.Abs(stick.x) > Mathf.Abs(h)) h = stick.x;
                if (Mathf.Abs(stick.y) > Mathf.Abs(v)) v = stick.y;
            }
#else
            // Legacy Input Manager path.
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");
#endif

            Vector2 input = new Vector2(h, v);
            // Normalize so diagonal movement isn't faster.
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }
            return input;
        }

        private Vector3 ClampToScreen(Vector3 worldPosition)
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                {
                    return worldPosition;
                }
            }

            Vector3 min = _camera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
            Vector3 max = _camera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

            worldPosition.x = Mathf.Clamp(worldPosition.x, min.x + screenPadding, max.x - screenPadding);
            worldPosition.y = Mathf.Clamp(worldPosition.y, min.y + screenPadding, max.y - screenPadding);
            worldPosition.z = 0f;

            return worldPosition;
        }

        /// <summary>
        /// Temporarily multiplies movement speed. A new call resets the timer.
        /// </summary>
        public void ApplySpeedBoost(float multiplier, float duration)
        {
            if (_speedBoostRoutine != null)
            {
                StopCoroutine(_speedBoostRoutine);
            }
            _speedBoostRoutine = StartCoroutine(SpeedBoostRoutine(multiplier, duration));
        }

        private IEnumerator SpeedBoostRoutine(float multiplier, float duration)
        {
            _currentSpeedMultiplier = multiplier;
            yield return new WaitForSeconds(duration);
            _currentSpeedMultiplier = 1f;
            _speedBoostRoutine = null;
        }

        /// <summary>
        /// Resets speed to the configured base value and cancels any active boost.
        /// </summary>
        public void ResetSpeed()
        {
            if (_speedBoostRoutine != null)
            {
                StopCoroutine(_speedBoostRoutine);
                _speedBoostRoutine = null;
            }
            moveSpeed = _baseMoveSpeed;
            _currentSpeedMultiplier = 1f;
        }
    }
}
