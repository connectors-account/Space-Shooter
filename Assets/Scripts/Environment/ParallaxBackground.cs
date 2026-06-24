using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Vertically scrolling parallax background. Supports multiple layers, each
    /// scrolling at its own speed to create a depth effect. Each layer needs two
    /// stacked copies of the sprite so it can loop seamlessly.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [System.Serializable]
        public class ParallaxLayer
        {
            public Transform layerRoot;     // parent containing two stacked tiles
            public float scrollSpeed = 1f;
            [HideInInspector] public float tileHeight;
            [HideInInspector] public Transform tileA;
            [HideInInspector] public Transform tileB;
        }

        [SerializeField] private ParallaxLayer[] layers;

        private void Start()
        {
            foreach (var layer in layers)
            {
                InitLayer(layer);
            }
        }

        private void InitLayer(ParallaxLayer layer)
        {
            if (layer.layerRoot == null || layer.layerRoot.childCount < 2) return;

            layer.tileA = layer.layerRoot.GetChild(0);
            layer.tileB = layer.layerRoot.GetChild(1);

            SpriteRenderer sr = layer.tileA.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                layer.tileHeight = sr.bounds.size.y;
            }

            // Position tile B directly above tile A.
            layer.tileB.position = layer.tileA.position + Vector3.up * layer.tileHeight;
        }

        private void Update()
        {
            foreach (var layer in layers)
            {
                ScrollLayer(layer);
            }
        }

        private void ScrollLayer(ParallaxLayer layer)
        {
            if (layer.tileA == null || layer.tileB == null) return;

            Vector3 delta = Vector3.down * layer.scrollSpeed * Time.deltaTime;
            layer.tileA.position += delta;
            layer.tileB.position += delta;

            RecycleTile(layer, layer.tileA);
            RecycleTile(layer, layer.tileB);
        }

        private void RecycleTile(ParallaxLayer layer, Transform tile)
        {
            // When a tile fully scrolls below the camera, move it back up above
            // the other tile to create an infinite loop.
            if (Camera.main == null) return;

            float camBottom = Camera.main.ViewportToWorldPoint(Vector3.zero).y;
            if (tile.position.y + layer.tileHeight * 0.5f < camBottom)
            {
                Transform other = tile == layer.tileA ? layer.tileB : layer.tileA;
                tile.position = new Vector3(tile.position.x, other.position.y + layer.tileHeight, tile.position.z);
            }
        }
    }
}
