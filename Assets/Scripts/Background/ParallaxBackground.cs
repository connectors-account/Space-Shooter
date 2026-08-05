using System.Collections.Generic;
using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Background
{
    /// <summary>
    /// A single scrolling layer. Each layer uses two stacked sprite tiles so
    /// it can wrap seamlessly: when one tile scrolls fully off the bottom it
    /// is repositioned above the other.
    /// </summary>
    [System.Serializable]
    public class ParallaxLayer
    {
        [Tooltip("Optional pre-assigned SpriteRenderer. If null, a star-field is generated procedurally.")]
        public SpriteRenderer spriteRenderer;

        [Tooltip("Downward scroll speed in world units per second.")]
        public float scrollSpeed = 1f;

        [Tooltip("Sorting order for the generated layer (lower = further back).")]
        public int sortingOrder = -100;

        [Tooltip("Number of stars per generated tile.")]
        public int starCount = 60;

        [Tooltip("Tint applied to generated stars.")]
        public Color tint = Color.white;

        // Runtime: the two tiles that make up this layer.
        [System.NonSerialized] public Transform tileA;
        [System.NonSerialized] public Transform tileB;
        [System.NonSerialized] public float tileHeight;
    }

    /// <summary>
    /// Multi-layer parallax background. Attach to a parent GameObject and add
    /// layers in the Inspector, or leave empty to auto-generate three star
    /// layers. Scrolls each layer downward at its own speed and tiles it
    /// seamlessly. Star-field sprites are generated procedurally if none are
    /// assigned, so no art assets are required.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [SerializeField] private List<ParallaxLayer> layers = new List<ParallaxLayer>();

        [Tooltip("If true and no layers are configured, three default star layers are created.")]
        [SerializeField] private bool autoGenerateIfEmpty = true;

        private Camera _camera;
        private float _viewHeight;
        private float _viewWidth;

        private void Start()
        {
            _camera = Camera.main;
            if (_camera == null) _camera = FindObjectOfType<Camera>();

            ComputeViewSize();

            if (layers.Count == 0 && autoGenerateIfEmpty)
                CreateDefaultLayers();

            foreach (var layer in layers)
                InitialiseLayer(layer);
        }

        private void ComputeViewSize()
        {
            if (_camera != null && _camera.orthographic)
            {
                _viewHeight = _camera.orthographicSize * 2f;
                _viewWidth = _viewHeight * _camera.aspect;
            }
            else
            {
                // Sensible fallback for a portrait shooter.
                _viewHeight = 10f;
                _viewWidth = 6f;
            }
        }

        private void CreateDefaultLayers()
        {
            layers.Add(new ParallaxLayer { scrollSpeed = 0.6f, sortingOrder = -120, starCount = 40, tint = new Color(0.6f, 0.6f, 0.8f) });
            layers.Add(new ParallaxLayer { scrollSpeed = 1.4f, sortingOrder = -110, starCount = 55, tint = new Color(0.85f, 0.85f, 1f) });
            layers.Add(new ParallaxLayer { scrollSpeed = 2.6f, sortingOrder = -100, starCount = 70, tint = Color.white });
        }

        private void InitialiseLayer(ParallaxLayer layer)
        {
            if (layer.spriteRenderer == null)
            {
                // Build a procedural star-field tile.
                var tileSprite = GenerateStarFieldSprite(layer);
                layer.spriteRenderer = CreateTileRenderer("StarLayer", tileSprite, layer);
            }

            layer.tileHeight = layer.spriteRenderer.bounds.size.y;
            if (layer.tileHeight <= 0.001f) layer.tileHeight = _viewHeight;

            // Set up two stacked tiles for seamless wrapping.
            layer.tileA = layer.spriteRenderer.transform;
            layer.tileA.position = new Vector3(transform.position.x, 0f, layer.tileA.position.z);

            var second = Instantiate(layer.spriteRenderer.gameObject, transform);
            second.name = layer.spriteRenderer.name + "_B";
            layer.tileB = second.transform;
            layer.tileB.position = new Vector3(transform.position.x, layer.tileHeight, layer.tileB.position.z);
        }

        private SpriteRenderer CreateTileRenderer(string name, Sprite sprite, ParallaxLayer layer)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = layer.sortingOrder;
            sr.color = layer.tint;
            return sr;
        }

        /// <summary>
        /// Draw a star-field the size of the current view onto a texture and
        /// return it as a sprite. Uses the shared star sprite pixel style.
        /// </summary>
        private Sprite GenerateStarFieldSprite(ParallaxLayer layer)
        {
            const int ppu = 32;
            int texW = Mathf.Max(32, Mathf.RoundToInt(_viewWidth * ppu));
            int texH = Mathf.Max(32, Mathf.RoundToInt(_viewHeight * ppu));

            var tex = new Texture2D(texW, texH, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var clear = new Color32(0, 0, 0, 0);
            var pixels = new Color32[texW * texH];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;
            tex.SetPixels32(pixels);

            var rng = new System.Random(layer.GetHashCode() ^ layer.sortingOrder);
            for (int s = 0; s < layer.starCount; s++)
            {
                int x = rng.Next(0, texW);
                int y = rng.Next(0, texH);
                float brightness = 0.5f + (float)rng.NextDouble() * 0.5f;
                var col = new Color(brightness, brightness, brightness, 1f);

                int starSize = rng.Next(0, 100) < 20 ? 2 : 1; // Occasional bigger star.
                for (int dy = 0; dy < starSize; dy++)
                    for (int dx = 0; dx < starSize; dx++)
                    {
                        int px = Mathf.Clamp(x + dx, 0, texW - 1);
                        int py = Mathf.Clamp(y + dy, 0, texH - 1);
                        tex.SetPixel(px, py, col);
                    }
            }
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, texW, texH), new Vector2(0.5f, 0.5f), ppu);
        }

        private void Update()
        {
            if (Core.GameManager.Instance != null && Core.GameManager.Instance.IsPaused)
                return;

            float dt = Time.deltaTime;
            foreach (var layer in layers)
            {
                if (layer.tileA == null || layer.tileB == null) continue;

                float delta = layer.scrollSpeed * dt;
                layer.tileA.position += Vector3.down * delta;
                layer.tileB.position += Vector3.down * delta;

                RecycleIfNeeded(layer, layer.tileA, layer.tileB);
                RecycleIfNeeded(layer, layer.tileB, layer.tileA);
            }
        }

        private void RecycleIfNeeded(ParallaxLayer layer, Transform tile, Transform other)
        {
            // When a tile passes fully below the view, lift it above the other.
            float bottomEdge = -_viewHeight * 0.5f - layer.tileHeight * 0.5f;
            if (tile.position.y < bottomEdge)
            {
                tile.position = new Vector3(
                    other.position.x,
                    other.position.y + layer.tileHeight,
                    tile.position.z);
            }
        }
    }
}
