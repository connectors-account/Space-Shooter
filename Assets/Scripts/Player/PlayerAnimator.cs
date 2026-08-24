using UnityEngine;
using SpaceShooter.Core;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SpaceShooter.Player
{
    /// <summary>
    /// Tilts the player ship left/right based on horizontal input and lerps back to
    /// centre when there is no input. Purely cosmetic.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("Tilt")]
        [SerializeField] private float tiltAngle = 15f;
        [SerializeField] private float tiltSpeed = 5f;

        private float _currentTilt;

        private void Update()
        {
            float horizontal = ReadHorizontal();

            // Tilting around the Z axis. Moving right tilts the nose to the right (negative Z).
            float targetTilt = -horizontal * tiltAngle;
            _currentTilt = Mathf.Lerp(_currentTilt, targetTilt, tiltSpeed * Time.deltaTime);

            transform.localRotation = Quaternion.Euler(0f, 0f, _currentTilt);
        }

        private float ReadHorizontal()
        {
            if (GameManager.HasInstance && GameManager.Instance.State != GameState.Playing)
            {
                return 0f;
            }

#if ENABLE_INPUT_SYSTEM
            float h = 0f;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) h -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) h += 1f;
            }
            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                float stickX = gamepad.leftStick.ReadValue().x;
                if (Mathf.Abs(stickX) > Mathf.Abs(h)) h = stickX;
            }
            return h;
#else
            return Input.GetAxis("Horizontal");
#endif
        }
    }
}
