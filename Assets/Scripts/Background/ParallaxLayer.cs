using UnityEngine;

namespace SpaceShooter.Background
{
    /// <summary>
    /// A single scrolling parallax layer. Moves downward each frame and loops seamlessly
    /// when it exits the bottom of the screen. Designed to work with a duplicated pair of
    /// tiled sprites, or with a single tall tiled SpriteRenderer.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ParallaxLayer : MonoBehaviour
    {
        [Header("Scrolling")]
        [SerializeField] private float scrollSpeed = 1f;

        [Tooltip("Optional second sprite used for the two-tile loop. If left empty, a single-sprite wrap is used.")]
        [SerializeField] private Transform secondTile;

        private SpriteRenderer _spriteRenderer;
        private float _spriteHeight;
        private float _resetY;
        private float _topY;

        public float ScrollSpeed
        {
            get => scrollSpeed;
            set => scrollSpeed = value;
        }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            CalculateBounds();
        }

        private void CalculateBounds()
        {
            if (_spriteRenderer != null && _spriteRenderer.sprite != null)
            {
                _spriteHeight = _spriteRenderer.bounds.size.y;
            }
            else
            {
                _spriteHeight = 10f;
            }

            Camera cam = Camera.main;
            float camHeight = cam != null ? cam.orthographicSize * 2f : 10f;

            // When the sprite's top edge falls below the camera bottom, wrap it back up.
            _resetY = -camHeight - _spriteHeight * 0.5f;
            _topY = _spriteHeight;
        }

        private void Update()
        {
            float delta = scrollSpeed * Time.deltaTime;

            MoveAndWrap(transform, delta);
            if (secondTile != null)
            {
                MoveAndWrap(secondTile, delta);
            }
        }

        private void MoveAndWrap(Transform tile, float delta)
        {
            Vector3 pos = tile.position;
            pos.y -= delta;

            // Wrap to the top once below the reset threshold.
            if (pos.y <= _resetY)
            {
                if (secondTile != null)
                {
                    // Two-tile setup: place this tile a full height above the highest tile.
                    float highestY = Mathf.Max(transform.position.y, secondTile.position.y);
                    pos.y = highestY + _spriteHeight;
                }
                else
                {
                    // Single tile wrap.
                    pos.y += _spriteHeight + _topY;
                }
            }

            tile.position = pos;
        }
    }
}
