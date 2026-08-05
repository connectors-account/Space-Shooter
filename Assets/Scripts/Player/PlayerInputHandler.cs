using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Wraps the Unity new Input System. Input actions are created in code (no
    /// external .inputactions asset required) and bound to keyboard/mouse and
    /// gamepad. Other components read the exposed properties each frame.
    ///
    /// Exposed state:
    ///   MoveInput   – Vector2 movement axis (WASD / arrows / left stick / D-pad)
    ///   FireHeld    – true while the fire control is held (for auto-fire)
    ///   FirePressed – true on the frame fire was pressed
    ///   BombPressed – true on the frame the bomb control was pressed
    ///   PausePressed– true on the frame pause was pressed
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        private InputAction _moveAction;
        private InputAction _fireAction;
        private InputAction _bombAction;
        private InputAction _pauseAction;

        public Vector2 MoveInput { get; private set; }
        public bool FireHeld { get; private set; }
        public bool FirePressed { get; private set; }
        public bool BombPressed { get; private set; }
        public bool PausePressed { get; private set; }

        private void Awake()
        {
            // --- Movement (Vector2 composite) ---
            _moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            _moveAction.AddBinding("<Gamepad>/leftStick");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Gamepad>/dpad/up")
                .With("Down", "<Gamepad>/dpad/down")
                .With("Left", "<Gamepad>/dpad/left")
                .With("Right", "<Gamepad>/dpad/right");

            // --- Fire ---
            _fireAction = new InputAction("Fire", InputActionType.Button);
            _fireAction.AddBinding("<Keyboard>/space");
            _fireAction.AddBinding("<Mouse>/leftButton");
            _fireAction.AddBinding("<Gamepad>/buttonSouth");
            _fireAction.AddBinding("<Gamepad>/rightTrigger");

            // --- Bomb ---
            _bombAction = new InputAction("Bomb", InputActionType.Button);
            _bombAction.AddBinding("<Keyboard>/b");
            _bombAction.AddBinding("<Keyboard>/leftShift");
            _bombAction.AddBinding("<Mouse>/rightButton");
            _bombAction.AddBinding("<Gamepad>/buttonWest");

            // --- Pause ---
            _pauseAction = new InputAction("Pause", InputActionType.Button);
            _pauseAction.AddBinding("<Keyboard>/escape");
            _pauseAction.AddBinding("<Keyboard>/p");
            _pauseAction.AddBinding("<Gamepad>/start");
        }

        private void OnEnable()
        {
            _moveAction.Enable();
            _fireAction.Enable();
            _bombAction.Enable();
            _pauseAction.Enable();
        }

        private void OnDisable()
        {
            _moveAction.Disable();
            _fireAction.Disable();
            _bombAction.Disable();
            _pauseAction.Disable();
        }

        private void OnDestroy()
        {
            _moveAction?.Dispose();
            _fireAction?.Dispose();
            _bombAction?.Dispose();
            _pauseAction?.Dispose();
        }

        private void Update()
        {
            MoveInput = _moveAction.ReadValue<Vector2>();
            FireHeld = _fireAction.IsPressed();
            FirePressed = _fireAction.WasPressedThisFrame();
            BombPressed = _bombAction.WasPressedThisFrame();
            PausePressed = _pauseAction.WasPressedThisFrame();
        }
    }
}
