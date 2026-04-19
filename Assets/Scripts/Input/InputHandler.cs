using UnityEngine;

namespace SpaceShooter.Input
{
    /// <summary>
    /// Centralized keyboard input reader.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        public static InputHandler Instance { get; private set; }

        public Vector2 MoveInput { get; private set; }
        public bool ShootHeld { get; private set; }
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
            float horizontal = UnityEngine.Input.GetAxisRaw("Horizontal");
            float vertical = UnityEngine.Input.GetAxisRaw("Vertical");
            MoveInput = new Vector2(horizontal, vertical).normalized;

            ShootHeld = UnityEngine.Input.GetKey(KeyCode.Space);
            PausePressedThisFrame = UnityEngine.Input.GetKeyDown(KeyCode.Escape);
        }
    }
}
