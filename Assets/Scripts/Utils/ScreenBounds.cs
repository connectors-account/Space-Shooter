using UnityEngine;

namespace SpaceShooter.Utils
{
    /// <summary>
    /// Utility class to calculate and provide screen boundaries in world space.
    /// Attach to the main camera or any persistent object.
    /// </summary>
    public class ScreenBounds : MonoBehaviour
    {
        public static ScreenBounds Instance { get; private set; }

        public float MinX { get; private set; }
        public float MaxX { get; private set; }
        public float MinY { get; private set; }
        public float MaxY { get; private set; }

        private Camera mainCamera;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            mainCamera = Camera.main;
            CalculateBounds();
        }

        private void CalculateBounds()
        {
            if (mainCamera == null) return;

            // Convert screen corners to world space
            Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
            Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

            MinX = bottomLeft.x;
            MaxX = topRight.x;
            MinY = bottomLeft.y;
            MaxY = topRight.y;
        }

        /// <summary>Check if a position is within screen bounds with optional padding.</summary>
        public bool IsWithinBounds(Vector3 position, float padding = 0f)
        {
            return position.x >= MinX - padding && position.x <= MaxX + padding &&
                   position.y >= MinY - padding && position.y <= MaxY + padding;
        }

        /// <summary>Clamp a position to stay within screen bounds.</summary>
        public Vector3 ClampToBounds(Vector3 position, float padding = 0.5f)
        {
            position.x = Mathf.Clamp(position.x, MinX + padding, MaxX - padding);
            position.y = Mathf.Clamp(position.y, MinY + padding, MaxY - padding);
            return position;
        }
    }
}
