// =============================================================================
// InputHandler.cs — Centralized input management
// =============================================================================
using UnityEngine;
using System;

namespace SpaceShooter.Managers
{
    /// <summary>
    /// Centralizes all game input, providing events and properties
    /// that other systems can query or subscribe to.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        public static InputHandler Instance { get; private set; }

        /// <summary>Normalized movement vector (WASD/Arrow keys).</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>Whether the fire button is held.</summary>
        public bool IsFiring { get; private set; }

        /// <summary>Whether pause was pressed this frame.</summary>
        public bool PausePressed { get; private set; }

        /// <summary>Event fired when pause is toggled.</summary>
        public event Action OnPauseToggle;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // Movement
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            MoveInput = new Vector2(h, v).normalized;

            // Firing
            IsFiring = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);

            // Pause
            PausePressed = Input.GetKeyDown(KeyCode.Escape);
            if (PausePressed)
            {
                OnPauseToggle?.Invoke();
            }
        }
    }
}
