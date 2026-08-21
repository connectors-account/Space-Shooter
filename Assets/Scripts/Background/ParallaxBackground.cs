using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Background
{
    /// <summary>
    /// Scrolls three star layers at different speeds for a parallax effect, with
    /// seamless vertical tiling. Each layer needs two stacked SpriteRenderers so
    /// one can wrap above the other as they scroll down.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [System.Serializable]
        public class ParallaxLayer
        {
            public Transform tileA;
            public Transform tileB;
            public float speed = 1f;
            [HideInInspector] public float tileHeight;
        }

        [SerializeField]
        private ParallaxLayer farLayer = new ParallaxLayer { speed = 0.5f };
        [SerializeField]
        private ParallaxLayer midLayer = new ParallaxLayer { speed = 1.5f };
        [SerializeField]
        private ParallaxLayer nearLayer = new ParallaxLayer { speed = 3.0f };

        [SerializeField] private float fallbackTileHeight = 10f;

        private ParallaxLayer[] _layers;

        private void Start()
        {
            _layers = new[] { farLayer, midLayer, nearLayer };
            foreach (var layer in _layers)
            {
                InitLayer(layer);
            }
        }

        private void InitLayer(ParallaxLayer layer)
        {
            if (layer.tileA == null) return;

            layer.tileHeight = MeasureHeight(layer.tileA);
            if (layer.tileHeight <= 0.01f)
            {
                layer.tileHeight = fallbackTileHeight;
            }

            // Stack tileB directly above tileA for seamless wrapping.
            if (layer.tileB != null)
            {
                Vector3 p = layer.tileA.position;
                p.y += layer.tileHeight;
                layer.tileB.position = p;
            }
        }

        private float MeasureHeight(Transform tile)
        {
            var sr = tile.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                return sr.bounds.size.y;
            }
            return fallbackTileHeight;
        }

        private void Update()
        {
            if (_layers == null) return;
            foreach (var layer in _layers)
            {
                ScrollLayer(layer);
            }
        }

        private void ScrollLayer(ParallaxLayer layer)
        {
            if (layer.tileA == null) return;

            float delta = layer.speed * Time.deltaTime;
            MoveAndWrap(layer.tileA, layer, delta);
            if (layer.tileB != null)
            {
                MoveAndWrap(layer.tileB, layer, delta);
            }
        }

        private void MoveAndWrap(Transform tile, ParallaxLayer layer, float delta)
        {
            Vector3 pos = tile.position;
            pos.y -= delta;

            // When a tile falls fully below the view, wrap it above the other tile.
            float bottomLimit = ScreenBounds.Instance != null
                ? ScreenBounds.Instance.MinY - layer.tileHeight * 0.5f
                : -fallbackTileHeight;

            if (pos.y < bottomLimit)
            {
                pos.y += layer.tileHeight * 2f;
            }

            tile.position = pos;
        }
    }
}
