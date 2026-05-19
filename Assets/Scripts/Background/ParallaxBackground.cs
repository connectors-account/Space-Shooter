using UnityEngine;

/// <summary>
/// Creates a parallax scrolling starfield background using dynamically
/// generated star layers that scroll at different speeds for depth effect.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Background Settings")]
    [SerializeField] private Color backgroundColor = new Color(0.02f, 0.02f, 0.08f, 1f);

    [Header("Star Layers")]
    [SerializeField] private int farStarCount = 80;
    [SerializeField] private float farStarSpeed = 0.5f;
    [SerializeField] private float farStarSize = 0.03f;

    [SerializeField] private int midStarCount = 40;
    [SerializeField] private float midStarSpeed = 1.2f;
    [SerializeField] private float midStarSize = 0.06f;

    [SerializeField] private int nearStarCount = 20;
    [SerializeField] private float nearStarSpeed = 2.5f;
    [SerializeField] private float nearStarSize = 0.1f;

    private StarLayer[] starLayers;
    private float screenHeight;
    private float screenWidth;

    private struct Star
    {
        public GameObject obj;
        public SpriteRenderer renderer;
    }

    private class StarLayer
    {
        public Star[] stars;
        public float speed;
    }

    private void Start()
    {
        Camera.main.backgroundColor = backgroundColor;

        screenHeight = Camera.main.orthographicSize * 2f;
        screenWidth = screenHeight * Camera.main.aspect;

        starLayers = new StarLayer[3];
        starLayers[0] = CreateStarLayer(farStarCount, farStarSpeed, farStarSize, new Color(0.4f, 0.4f, 0.6f, 0.5f), -5);
        starLayers[1] = CreateStarLayer(midStarCount, midStarSpeed, midStarSize, new Color(0.6f, 0.6f, 0.8f, 0.7f), -4);
        starLayers[2] = CreateStarLayer(nearStarCount, nearStarSpeed, nearStarSize, new Color(0.9f, 0.9f, 1f, 0.9f), -3);
    }

    private void Update()
    {
        if (starLayers == null) return;

        foreach (var layer in starLayers)
        {
            if (layer == null) continue;

            foreach (var star in layer.stars)
            {
                if (star.obj == null) continue;

                // Scroll star downward
                star.obj.transform.Translate(Vector3.down * layer.speed * Time.deltaTime, Space.World);

                // Wrap star to top when it goes below screen
                if (star.obj.transform.position.y < -(screenHeight / 2f + 1f))
                {
                    float newX = Random.Range(-screenWidth / 2f, screenWidth / 2f);
                    star.obj.transform.position = new Vector3(newX, screenHeight / 2f + 1f, 0);
                }
            }
        }
    }

    /// <summary>
    /// Creates a layer of star GameObjects with the given properties.
    /// </summary>
    private StarLayer CreateStarLayer(int count, float speed, float size, Color color, int sortOrder)
    {
        StarLayer layer = new StarLayer();
        layer.speed = speed;
        layer.stars = new Star[count];

        GameObject layerParent = new GameObject($"StarLayer_{sortOrder}");
        layerParent.transform.SetParent(transform);

        Sprite starSprite = CreateStarSprite();

        for (int i = 0; i < count; i++)
        {
            GameObject starObj = new GameObject($"Star_{i}");
            starObj.transform.SetParent(layerParent.transform);

            float x = Random.Range(-screenWidth / 2f, screenWidth / 2f);
            float y = Random.Range(-screenHeight / 2f, screenHeight / 2f);
            starObj.transform.position = new Vector3(x, y, 0);
            starObj.transform.localScale = Vector3.one * size * Random.Range(0.5f, 1.5f);

            SpriteRenderer sr = starObj.AddComponent<SpriteRenderer>();
            sr.sprite = starSprite;
            sr.color = color;
            sr.sortingOrder = sortOrder;

            layer.stars[i] = new Star { obj = starObj, renderer = sr };
        }

        return layer;
    }

    /// <summary>
    /// Creates a simple white circle sprite for stars.
    /// </summary>
    private Sprite CreateStarSprite()
    {
        int res = 8;
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color clear = new Color(1f, 1f, 1f, 0f);
        float center = res / 2f;
        float radius = center - 0.5f;

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= radius)
                {
                    float alpha = 1f - (dist / radius) * 0.5f;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, clear);
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), Vector2.one * 0.5f, res);
    }
}
