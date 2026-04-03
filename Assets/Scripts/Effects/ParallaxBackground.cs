using UnityEngine;

/// <summary>
/// Creates a scrolling starfield parallax background.
/// Generates star layers at different speeds for depth effect.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Star Field Settings")]
    public int starCount = 100;
    public float[] layerSpeeds = { 0.5f, 1f, 2f };
    public Color[] layerColors;
    public float[] layerSizes = { 0.02f, 0.04f, 0.06f };

    private GameObject[][] starLayers;
    private float screenHeight;
    private float screenWidth;

    void Start()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        screenHeight = cam.orthographicSize * 2f;
        screenWidth = screenHeight * cam.aspect;

        if (layerColors == null || layerColors.Length == 0)
        {
            layerColors = new Color[]
            {
                new Color(0.3f, 0.3f, 0.4f),
                new Color(0.5f, 0.5f, 0.7f),
                new Color(0.8f, 0.8f, 1.0f)
            };
        }

        CreateStarField();
    }

    void CreateStarField()
    {
        starLayers = new GameObject[layerSpeeds.Length][];

        for (int layer = 0; layer < layerSpeeds.Length; layer++)
        {
            int count = starCount / layerSpeeds.Length;
            starLayers[layer] = new GameObject[count];

            GameObject layerParent = new GameObject($"StarLayer_{layer}");
            layerParent.transform.SetParent(transform);

            Color color = layer < layerColors.Length ? layerColors[layer] : Color.white;
            float size = layer < layerSizes.Length ? layerSizes[layer] : 0.04f;

            Sprite starSprite = CreateStarSprite(color);

            for (int i = 0; i < count; i++)
            {
                GameObject star = new GameObject($"Star_{i}");
                star.transform.SetParent(layerParent.transform);
                star.transform.position = new Vector3(
                    Random.Range(-screenWidth / 2f - 1f, screenWidth / 2f + 1f),
                    Random.Range(-screenHeight / 2f - 2f, screenHeight / 2f + 2f),
                    0f
                );
                star.transform.localScale = Vector3.one * size * (16f / 4f);

                SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
                sr.sprite = starSprite;
                sr.sortingOrder = -10 + layer;

                starLayers[layer][i] = star;
            }
        }
    }

    void Update()
    {
        if (starLayers == null) return;

        for (int layer = 0; layer < starLayers.Length; layer++)
        {
            float speed = layerSpeeds[layer];
            for (int i = 0; i < starLayers[layer].Length; i++)
            {
                GameObject star = starLayers[layer][i];
                if (star == null) continue;

                star.transform.position += Vector3.down * speed * Time.deltaTime;

                // Wrap around when off bottom of screen
                if (star.transform.position.y < -screenHeight / 2f - 1f)
                {
                    star.transform.position = new Vector3(
                        Random.Range(-screenWidth / 2f, screenWidth / 2f),
                        screenHeight / 2f + 1f,
                        0f
                    );
                }
            }
        }
    }

    Sprite CreateStarSprite(Color color)
    {
        int size = 4;
        Texture2D tex = new Texture2D(size, size);
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, color);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
    }
}
