// =============================================================================
// ScreenWrapper.cs — Wraps objects around screen edges
// =============================================================================
using UnityEngine;

namespace SpaceShooter.Utils
{
    /// <summary>
    /// Wraps the object to the opposite screen edge when it exits.
    /// Useful for asteroids or certain enemy types.
    /// </summary>
    public class ScreenWrapper : MonoBehaviour
    {
        [SerializeField] private bool wrapHorizontal = true;
        [SerializeField] private bool wrapVertical = false;
        [SerializeField] private float padding = 0.5f;

        private float screenWidth;
        private float screenHeight;

        private void Start()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                screenHeight = cam.orthographicSize + padding;
                screenWidth = screenHeight * cam.aspect;
            }
        }

        private void Update()
        {
            Vector3 pos = transform.position;

            if (wrapHorizontal)
            {
                if (pos.x > screenWidth) pos.x = -screenWidth;
                else if (pos.x < -screenWidth) pos.x = screenWidth;
            }

            if (wrapVertical)
            {
                if (pos.y > screenHeight) pos.y = -screenHeight;
                else if (pos.y < -screenHeight) pos.y = screenHeight;
            }

            transform.position = pos;
        }
    }
}
