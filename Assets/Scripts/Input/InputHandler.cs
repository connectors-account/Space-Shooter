using UnityEngine;

namespace SpaceShooter.InputSystem
{
    /// <summary>
    /// Centralized input reader for player movement, firing and pause.
    /// Uses Unity's default Input Manager axes/buttons so no package setup is required.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        public static InputHandler Instance { get; private set; }

        public Vector2 MoveInput { get; private set; }
        public bool IsFireHeld { get; private set; }
        public bool PausePressedThisFrame { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            IsFireHeld = Input.GetKey(KeyCode.Space);
            PausePressedThisFrame = Input.GetKeyDown(KeyCode.Escape);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
