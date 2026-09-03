using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Environment
{
    /// <summary>
    /// Scrolls two stacked sprite tiles downward to create an infinite parallax
    /// background. When a tile scrolls off the bottom it is repositioned above the other.
    /// </summary>
    [DisallowMultipleComponent]
    public class ParallaxLayer : MonoBehaviour
    {
        #region Fields
        [SerializeField] private float _scrollSpeed = 1f;
        [SerializeField] private float _parallaxFactor = 1f;
        [SerializeField] private SpriteRenderer _tileA;
        [SerializeField] private SpriteRenderer _tileB;

        private float _tileHeight;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            SetupTiles();
        }

        private void Update()
        {
            float delta = _scrollSpeed * _parallaxFactor * Time.deltaTime;
            ScrollTile(_tileA, delta);
            ScrollTile(_tileB, delta);
        }
        #endregion

        #region Setup
        private void SetupTiles()
        {
            if (_tileA == null || _tileB == null)
            {
                Debug.LogWarning("[ParallaxLayer] Tiles not assigned; parallax disabled.");
                enabled = false;
                return;
            }

            _tileHeight = _tileA.bounds.size.y;
            if (_tileHeight <= 0.01f) _tileHeight = GameConstants.CAMERA_TOP - GameConstants.CAMERA_BOTTOM;

            // Stack tile B directly above tile A.
            Vector3 aPos = _tileA.transform.position;
            _tileB.transform.position = new Vector3(aPos.x, aPos.y + _tileHeight, aPos.z);
        }
        #endregion

        #region Scrolling
        private void ScrollTile(SpriteRenderer tile, float delta)
        {
            if (tile == null) return;
            tile.transform.Translate(Vector3.down * delta, Space.World);

            float bottomEdge = GameConstants.CAMERA_BOTTOM - _tileHeight * 0.5f;
            if (tile.transform.position.y < bottomEdge)
            {
                SpriteRenderer other = tile == _tileA ? _tileB : _tileA;
                Vector3 pos = tile.transform.position;
                pos.y = other.transform.position.y + _tileHeight;
                tile.transform.position = pos;
            }
        }
        #endregion

        #region Configuration
        /// <summary>Configures scroll speed and parallax factor at runtime.</summary>
        public void Configure(float scrollSpeed, float parallaxFactor)
        {
            _scrollSpeed = scrollSpeed;
            _parallaxFactor = parallaxFactor;
        }
        #endregion
    }
}
