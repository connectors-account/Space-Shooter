using UnityEngine;

namespace SpaceShooter.Utilities
{
    /// <summary>
    /// Calculates world-space screen bounds from the main orthographic camera.
    /// Singleton so any script can query playfield limits cheaply.
    /// </summary>
    public class ScreenBounds : MonoBehaviour
    {
        public static ScreenBounds Instance { get; private set; }

        [Tooltip("Extra world-units of padding kept outside the visible area for spawning/despawning.")]
        public float margin = 1f;

        private Camera _cam;

        public float Left { get; private set; }
        public float Right { get; private set; }
        public float Top { get; private set; }
        public float Bottom { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            _cam = Camera.main;
            Recalculate();
        }

        private void Update()
        {
            // Recalculate every frame so it stays correct if the resolution changes.
            Recalculate();
        }

        public void Recalculate()
        {
            if (_cam == null) _cam = Camera.main;
            if (_cam == null) return;

            float halfHeight = _cam.orthographicSize;
            float halfWidth = halfHeight * _cam.aspect;
            Vector3 pos = _cam.transform.position;

            Left = pos.x - halfWidth;
            Right = pos.x + halfWidth;
            Top = pos.y + halfHeight;
            Bottom = pos.y - halfHeight;
        }

        /// <summary>True if a position is beyond the bounds plus margin (used to cull projectiles).</summary>
        public bool IsOutOfBounds(Vector3 pos)
        {
            return pos.x < Left - margin || pos.x > Right + margin ||
                   pos.y < Bottom - margin || pos.y > Top + margin;
        }

        /// <summary>Clamp a position to the visible playfield (used to keep the player on screen).</summary>
        public Vector3 Clamp(Vector3 pos, float padX = 0.5f, float padY = 0.5f)
        {
            pos.x = Mathf.Clamp(pos.x, Left + padX, Right - padX);
            pos.y = Mathf.Clamp(pos.y, Bottom + padY, Top - padY);
            return pos;
        }
    }
}
