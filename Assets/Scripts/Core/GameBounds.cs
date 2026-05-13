// ============================================================================
// GameBounds.cs — Keeps track of the visible play area
// Other scripts reference this to clamp the player and detect off-screen objects.
// ============================================================================
using UnityEngine;

namespace SpaceShooter.Core
{
    public class GameBounds : MonoBehaviour
    {
        public static GameBounds Instance { get; private set; }

        /// <summary>World-space boundaries derived from the orthographic camera.</summary>
        public float Left { get; private set; }
        public float Right { get; private set; }
        public float Top { get; private set; }
        public float Bottom { get; private set; }

        [Tooltip("Extra padding beyond the visible screen for spawning / despawning.")]
        [SerializeField] private float padding = 1f;

        private void Awake()
        {
            Instance = this;
            RecalculateBounds();
        }

        /// <summary>Recalculate if the camera changes (resolution swap, etc.).</summary>
        public void RecalculateBounds()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            Left   = cam.transform.position.x - halfWidth  - padding;
            Right  = cam.transform.position.x + halfWidth  + padding;
            Top    = cam.transform.position.y + halfHeight + padding;
            Bottom = cam.transform.position.y - halfHeight - padding;
        }

        /// <summary>Clamp position within visible screen (no padding).</summary>
        public Vector2 ClampToScreen(Vector2 pos)
        {
            Camera cam = Camera.main;
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            pos.x = Mathf.Clamp(pos.x, -halfWidth + 0.5f, halfWidth - 0.5f);
            pos.y = Mathf.Clamp(pos.y, -halfHeight + 0.5f, halfHeight - 0.5f);
            return pos;
        }

        /// <summary>Is the position outside the padded area?</summary>
        public bool IsOutOfBounds(Vector2 pos)
        {
            return pos.x < Left || pos.x > Right || pos.y < Bottom || pos.y > Top;
        }
    }
}
