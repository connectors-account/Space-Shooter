using UnityEngine;

namespace SpaceShooter.Utilities
{
    /// <summary>
    /// Singleton that calculates world-space screen bounds from the main Camera.
    /// Recalculates automatically if the screen resolution changes.
    /// </summary>
    public class ScreenBounds : MonoBehaviour
    {
        public static ScreenBounds Instance { get; private set; }

        [SerializeField] private Camera targetCamera;

        private int _lastWidth;
        private int _lastHeight;

        public float MinX { get; private set; }
        public float MaxX { get; private set; }
        public float MinY { get; private set; }
        public float MaxY { get; private set; }

        public float Width => MaxX - MinX;
        public float Height => MaxY - MinY;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
            Recalculate();
        }

        private void Update()
        {
            if (Screen.width != _lastWidth || Screen.height != _lastHeight)
            {
                Recalculate();
            }
        }

        /// <summary>Recomputes the world-space bounds using the orthographic camera.</summary>
        public void Recalculate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null) return;
            }

            _lastWidth = Screen.width;
            _lastHeight = Screen.height;

            float halfHeight = targetCamera.orthographicSize;
            float halfWidth = halfHeight * targetCamera.aspect;
            Vector3 camPos = targetCamera.transform.position;

            MinX = camPos.x - halfWidth;
            MaxX = camPos.x + halfWidth;
            MinY = camPos.y - halfHeight;
            MaxY = camPos.y + halfHeight;
        }

        /// <summary>Clamps a world position to stay within the screen with a margin.</summary>
        public Vector3 Clamp(Vector3 position, float margin = 0f)
        {
            position.x = Mathf.Clamp(position.x, MinX + margin, MaxX - margin);
            position.y = Mathf.Clamp(position.y, MinY + margin, MaxY - margin);
            return position;
        }

        /// <summary>Returns true if the point is outside the screen by the given padding.</summary>
        public bool IsOutside(Vector3 position, float padding = 1f)
        {
            return position.x < MinX - padding || position.x > MaxX + padding ||
                   position.y < MinY - padding || position.y > MaxY + padding;
        }
    }
}
