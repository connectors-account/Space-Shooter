using UnityEngine;

namespace SpaceShooter.Systems
{
    /// <summary>
    /// Scrolls three background layers downward at different speeds for a parallax effect.
    /// Each layer is duplicated vertically and teleported for seamless tiling.
    /// If star sprites are provided, generates a procedural star field per layer.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [System.Serializable]
        public class Layer
        {
            public Transform layerTransform;
            [Range(0f, 2f)] public float speedMultiplier = 0.5f;
            [HideInInspector] public float height;
            [HideInInspector] public Transform tileA;
            [HideInInspector] public Transform tileB;
        }

        [Header("Layers (far -> near)")]
        [SerializeField] private Layer[] layers;
        [SerializeField] private float baseScrollSpeed = 2f;

        [Header("Procedural Star Field (optional)")]
        [SerializeField] private Sprite starSmallSprite;
        [SerializeField] private Sprite starLargeSprite;
        [SerializeField] private int starsPerLayer = 40;
        [SerializeField] private bool generateStars = true;

        private Camera cam;
        private float worldHeight;
        private float worldWidth;

        private void Awake()
        {
            cam = Camera.main;
            ComputeScreenSize();

            if (generateStars && starSmallSprite != null)
            {
                GenerateStarField();
            }
            SetupTiles();
        }

        private void ComputeScreenSize()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;
            worldHeight = cam.orthographicSize * 2f;
            worldWidth = worldHeight * cam.aspect;
        }

        private void GenerateStarField()
        {
            if (layers == null) return;
            for (int li = 0; li < layers.Length; li++)
            {
                Layer layer = layers[li];
                if (layer.layerTransform == null) continue;

                for (int i = 0; i < starsPerLayer; i++)
                {
                    GameObject star = new GameObject($"Star_{li}_{i}");
                    star.transform.SetParent(layer.layerTransform);
                    float x = Random.Range(-worldWidth * 0.5f, worldWidth * 0.5f);
                    float y = Random.Range(-worldHeight * 0.5f, worldHeight * 0.5f);
                    star.transform.localPosition = new Vector3(x, y, 0f);

                    SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
                    bool large = Random.value > 0.7f && starLargeSprite != null;
                    sr.sprite = large ? starLargeSprite : starSmallSprite;
                    sr.sortingOrder = -10 + li;
                    float alpha = Mathf.Lerp(0.3f, 1f, layer.speedMultiplier);
                    sr.color = new Color(1f, 1f, 1f, alpha);
                }
            }
        }

        private void SetupTiles()
        {
            if (layers == null) return;
            foreach (Layer layer in layers)
            {
                if (layer.layerTransform == null) continue;
                layer.height = worldHeight;
                layer.tileA = layer.layerTransform;

                // Create a duplicate placed directly above for seamless wrap.
                GameObject dup = new GameObject(layer.layerTransform.name + "_TileB");
                dup.transform.SetParent(layer.layerTransform.parent);
                dup.transform.position = layer.layerTransform.position + Vector3.up * worldHeight;
                dup.transform.localScale = layer.layerTransform.localScale;

                // Copy children (stars / renderers) into duplicate.
                CopyChildren(layer.layerTransform, dup.transform);
                CopyRenderer(layer.layerTransform.gameObject, dup);

                layer.tileB = dup.transform;
            }
        }

        private void CopyChildren(Transform source, Transform destination)
        {
            foreach (Transform child in source)
            {
                GameObject copy = Instantiate(child.gameObject, destination);
                copy.transform.localPosition = child.localPosition;
            }
        }

        private void CopyRenderer(GameObject source, GameObject destination)
        {
            SpriteRenderer srcRenderer = source.GetComponent<SpriteRenderer>();
            if (srcRenderer != null && srcRenderer.sprite != null)
            {
                SpriteRenderer dstRenderer = destination.AddComponent<SpriteRenderer>();
                dstRenderer.sprite = srcRenderer.sprite;
                dstRenderer.color = srcRenderer.color;
                dstRenderer.sortingOrder = srcRenderer.sortingOrder;
                dstRenderer.drawMode = srcRenderer.drawMode;
                dstRenderer.size = srcRenderer.size;
            }
        }

        private void Update()
        {
            if (layers == null) return;
            foreach (Layer layer in layers)
            {
                if (layer.layerTransform == null) continue;
                float delta = baseScrollSpeed * layer.speedMultiplier * Time.deltaTime;

                ScrollTile(layer.tileA, delta, layer.height);
                ScrollTile(layer.tileB, delta, layer.height);
            }
        }

        private void ScrollTile(Transform tile, float delta, float height)
        {
            if (tile == null) return;
            tile.position += Vector3.down * delta;

            // When a tile drops a full screen below, teleport above.
            if (tile.position.y <= -height)
            {
                tile.position += Vector3.up * (height * 2f);
            }
        }
    }
}
