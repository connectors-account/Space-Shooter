using UnityEngine;

/// <summary>
/// Creates a parallax scrolling starfield background.
/// Manages multiple layers of stars at different scroll speeds for depth.
/// Attach to an empty GameObject. Stars are generated procedurally.
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private Color backgroundColor = new Color(0.02f, 0.02f, 0.08f, 1f);

    [Header("Star Layers")]
    [SerializeField] private int farStarCount = 80;
    [SerializeField] private int midStarCount = 40;
    [SerializeField] private int nearStarCount = 20;

    [SerializeField] private float farStarSpeed = 0.5f;
    [SerializeField] private float midStarSpeed = 1.5f;
    [SerializeField] private float nearStarSpeed = 3f;

    [Header("Star Area")]
    [SerializeField] private float areaWidth = 22f;
    [SerializeField] private float areaHeight = 14f;

    private StarLayer[] starLayers;

    private struct Star
    {
        public GameObject obj;
        public float speed;
    }

    private struct StarLayer
    {
        public Star[] stars;
        public float speed;
        public float size;
        public Color color;
    }

    private void Awake()
    {
        Camera.main.backgroundColor = backgroundColor;
    }

    private void Start()
    {
        CreateStarLayers();
    }

    private void Update()
    {
        for (int i = 0; i < starLayers.Length; i++)
        {
            ScrollLayer(ref starLayers[i]);
        }
    }

    private void CreateStarLayers()
    {
        starLayers = new StarLayer[3];

        // Far stars: small, dim, slow
        starLayers[0] = CreateLayer("FarStars", farStarCount, farStarSpeed, 0.04f,
            new Color(0.5f, 0.5f, 0.7f, 0.5f), -5);

        // Mid stars: medium size and speed
        starLayers[1] = CreateLayer("MidStars", midStarCount, midStarSpeed, 0.07f,
            new Color(0.7f, 0.7f, 0.9f, 0.7f), -4);

        // Near stars: larger, brighter, faster
        starLayers[2] = CreateLayer("NearStars", nearStarCount, nearStarSpeed, 0.12f,
            new Color(1f, 1f, 1f, 0.9f), -3);
    }

    private StarLayer CreateLayer(string layerName, int count, float speed, float size, Color color, int sortingOrder)
    {
        GameObject parent = new GameObject(layerName);
        parent.transform.SetParent(transform);

        StarLayer layer = new StarLayer
        {
            stars = new Star[count],
            speed = speed,
            size = size,
            color = color
        };

        for (int i = 0; i < count; i++)
        {
            GameObject star = new GameObject("Star_" + i);
            star.transform.SetParent(parent.transform);

            SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
            sr.sprite = CreateStarSprite();
            sr.sortingOrder = sortingOrder;

            // Vary color slightly for visual interest
            Color starColor = color;
            starColor.r += Random.Range(-0.1f, 0.1f);
            starColor.g += Random.Range(-0.1f, 0.1f);
            starColor.b += Random.Range(-0.05f, 0.1f);
            sr.color = starColor;

            // Random position across the area
            float x = Random.Range(-areaWidth / 2f, areaWidth / 2f);
            float y = Random.Range(-areaHeight / 2f, areaHeight / 2f);
            star.transform.position = new Vector3(x, y, 0f);

            // Vary size slightly
            float s = size * Random.Range(0.5f, 1.5f);
            star.transform.localScale = Vector3.one * s;

            layer.stars[i] = new Star { obj = star, speed = speed };
        }

        return layer;
    }

    private void ScrollLayer(ref StarLayer layer)
    {
        for (int i = 0; i < layer.stars.Length; i++)
        {
            if (layer.stars[i].obj == null) continue;

            Transform t = layer.stars[i].obj.transform;
            t.position += Vector3.down * layer.speed * Time.deltaTime;

            // Wrap around when star goes below screen
            if (t.position.y < -areaHeight / 2f)
            {
                float x = Random.Range(-areaWidth / 2f, areaWidth / 2f);
                t.position = new Vector3(x, areaHeight / 2f, 0f);
            }
        }
    }

    /// <summary>
    /// Creates a simple white circle sprite for stars at runtime.
    /// </summary>
    private Sprite CreateStarSprite()
    {
        int texSize = 8;
        Texture2D tex = new Texture2D(texSize, texSize);
        tex.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[texSize * texSize];
        Vector2 center = new Vector2(texSize / 2f, texSize / 2f);
        float radius = texSize / 2f;

        for (int y = 0; y < texSize; y++)
        {
            for (int x = 0; x < texSize; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(1f - dist / radius);
                pixels[y * texSize + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, texSize, texSize), Vector2.one * 0.5f, texSize);
    }
}
