using UnityEngine;
using SpaceShooter.Core;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SpaceShooter.Player
{
    /// <summary>
    /// Drives player movement (WASD/arrows), clamps to camera bounds, reads fire
    /// input (Space / left mouse) and applies a velocity-based tilt animation.
    /// Reads legacy Input and the new Input System when available.
    /// </summary>
    [RequireComponent(typeof(PlayerShooter))]
    public class PlayerController : MonoBehaviour
    {
        #region Fields
        [SerializeField] private float _moveSpeed = GameConstants.PLAYER_MOVE_SPEED;
        [SerializeField] private Transform _shipVisual;

        private PlayerShooter _shooter;
        private float _lastHorizontal;
        #endregion

        #region Properties
        public float MoveSpeed => _moveSpeed;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _shooter = GetComponent<PlayerShooter>();
            if (_shipVisual == null) _shipVisual = transform;
            gameObject.tag = GameConstants.TAG_PLAYER;
            gameObject.layer = GameConstants.LAYER_ID_PLAYER;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing)
                return;

            Vector2 input = ReadMoveInput();
            Move(input);
            ApplyTilt(input.x);

            if (ReadFireInput())
                _shooter.TryFire();
        }
        #endregion

        #region Input
        private Vector2 ReadMoveInput()
        {
            float h = 0f;
            float v = 0f;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h += 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) v += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) v -= 1f;
            }
#endif
            // Fall back to / combine with legacy input axes for maximum compatibility.
            if (Mathf.Approximately(h, 0f)) h = Input.GetAxisRaw("Horizontal");
            if (Mathf.Approximately(v, 0f)) v = Input.GetAxisRaw("Vertical");

            Vector2 v2 = new Vector2(h, v);
            if (v2.sqrMagnitude > 1f) v2.Normalize();
            return v2;
        }

        private bool ReadFireInput()
        {
            bool fire = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed) fire = true;
            if (Mouse.current != null && Mouse.current.leftButton.isPressed) fire = true;
#endif
            return fire;
        }
        #endregion

        #region Movement
        private void Move(Vector2 input)
        {
            Vector3 delta = (Vector3)input * _moveSpeed * Time.deltaTime;
            Vector3 pos = transform.position + delta;

            pos.x = Mathf.Clamp(pos.x, GameConstants.CAMERA_LEFT, GameConstants.CAMERA_RIGHT);
            pos.y = Mathf.Clamp(pos.y, GameConstants.CAMERA_BOTTOM, GameConstants.CAMERA_TOP);

            transform.position = pos;
            _lastHorizontal = input.x;
        }

        private void ApplyTilt(float horizontal)
        {
            if (_shipVisual == null) return;
            float targetZ = -horizontal * GameConstants.PLAYER_TILT_MAX_ANGLE;
            Quaternion target = Quaternion.Euler(0f, 0f, targetZ);
            _shipVisual.localRotation = Quaternion.Lerp(_shipVisual.localRotation, target, 10f * Time.deltaTime);
        }
        #endregion

        #region Public API
        /// <summary>Sets the ship's movement speed (used by speed-boost power-up).</summary>
        public void SetMoveSpeed(float speed)
        {
            _moveSpeed = speed;
        }
        #endregion
    }
}
