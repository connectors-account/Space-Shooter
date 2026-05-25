using UnityEngine;

/// <summary>
/// Creates and scrolls a multi-layer parallax starfield background.
/// Generates star layers programmatically — no external assets needed.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Star Layers")]
    [SerializeField] private int layerCount = 3;
    [SerializeField] private int starsPerLayer = 60;
    [SerializeField] private float baseScrollSpeed = 1f;

    private StarLayer[] layers;
    private Camera mainCamera;
    private float screenHeight;

    private struct StarLayer
    {
        public GameObject container;
        public float scrollSpeed;
        public float resetY;
        public float startY;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) return;

        screenHeight = mainCamera.orthographicSize * 2f;
        float screenWidth = screenHeight * mainCamera.aspect;

        // Set background color to dark space
        mainCamera.backgroundColor = new Color(0.02f, 0.02f, 0.08f);

        layers = new StarLayer[layerCount];

        for (int i = 0; i < layerCount; i++)
        {
            float depth = (i + 1f) / layerCount; // 0.33, 0.67, 1.0
            float speed = baseScrollSpeed * depth;
            float starSize = 0.03f + depth * 0.06f;
            float brightness = 0.3f + depth * 0.7f;
            int count = starsPerLayer + (int)(starsPerLayer * depth * 0.5f);

            GameObject layerObj = new GameObject($"StarLayer_{i}");
            layerObj.transform.SetParent(transform);

            // Create two copies of stars for seamless scrolling
            CreateStars(layerObj.transform, count, screenWidth, screenHeight * 2f,
                starSize, brightness, -screenHeight);

            layers[i] = new StarLayer
            {
                container = layerObj,
                scrollSpeed = speed,
                resetY = -screenHeight,
                startY = 0
            };
        }
    }

    /// <summary>
    /// Creates randomly positioned star sprites as children of the given parent.
    /// </summary>
    private void CreateStars(Transform parent, int count, float width, float height,
        float size, float brightness, float yOffset)
    {
        Sprite starSprite = CreateStarSprite();

        for (int i = 0; i < count; i++)
        {
            GameObject star = new GameObject("Star");
            star.transform.SetParent(parent);

            float x = Random.Range(-width / 2f, width / 2f);
            float y = Random.Range(yOffset, yOffset + height);
            star.transform.localPosition = new Vector3(x, y, 0);
            star.transform.localScale = Vector3.one * size * Random.Range(0.5f, 1.5f);

            SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
            sr.sprite = starSprite;

            // Vary star colors slightly — white, blue-white, yellow-white
            float colorVariation = Random.value;
            Color starColor;
            if (colorVariation < 0.6f)
                starColor = new Color(brightness, brightness, brightness); // White
            else if (colorVariation < 0.8f)
                starColor = new Color(brightness * 0.7f, brightness * 0.8f, brightness); // Blue-ish
            else
                starColor = new Color(brightness, brightness * 0.9f, brightness * 0.7f); // Yellow-ish

            sr.color = starColor;
            sr.sortingOrder = -10 + (int)(brightness * 5);
        }
    }

    private void Update()
    {
        if (layers == null) return;

        for (int i = 0; i < layers.Length; i++)
        {
            StarLayer layer = layers[i];
            if (layer.container == null) continue;

            // Scroll down
            Vector3 pos = layer.container.transform.position;
            pos.y -= layer.scrollSpeed * Time.deltaTime;

            // Reset when scrolled too far
            if (pos.y < -screenHeight)
            {
                pos.y += screenHeight;
            }

            layer.container.transform.position = pos;
        }
    }

    /// <summary>
    /// Creates a small white circle sprite for a star.
    /// </summary>
    private Sprite CreateStarSprite()
    {
        int size = 8;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        float center = size / 2f;
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01(1f - (dist / radius));
                colors[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 8f);
    }
}
