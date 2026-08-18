using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Infinite vertical scrolling for one star-field layer. Uses two (or more)
    /// stacked tile copies and wraps a tile back to the top once it scrolls off the
    /// bottom. Attach one instance per layer and give each a different
    /// <see cref="scrollSpeed"/> (e.g. 0.5, 1.0, 2.0) for a parallax effect.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [Tooltip("Downward scroll speed in world units per second.")]
        public float scrollSpeed = 1f;

        [Tooltip("World-space height of a single tile copy.")]
        public float tileHeight = 10f;

        [Tooltip("The stacked tile copies. If empty, child transforms are used.")]
        public Transform[] tiles;

        private void Awake()
        {
            if (tiles == null || tiles.Length == 0)
            {
                CollectChildTiles();
            }
        }

        private void CollectChildTiles()
        {
            int count = transform.childCount;
            tiles = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                tiles[i] = transform.GetChild(i);
            }
        }

        private void Update()
        {
            if (tiles == null || tiles.Length == 0) return;

            float delta = scrollSpeed * Time.deltaTime;
            float wrap = tileHeight * tiles.Length;

            for (int i = 0; i < tiles.Length; i++)
            {
                Transform tile = tiles[i];
                if (tile == null) continue;

                Vector3 p = tile.localPosition;
                p.y -= delta;
                if (p.y <= -tileHeight)
                {
                    p.y += wrap;
                }
                tile.localPosition = p;
            }
        }
    }
}
