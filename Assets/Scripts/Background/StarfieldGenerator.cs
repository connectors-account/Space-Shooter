using UnityEngine;

/// <summary>
/// Generates a scrolling starfield effect using particle system or simple moving sprites.
/// Creates multiple star layers at different depths/speeds.
/// </summary>
public class StarfieldGenerator : MonoBehaviour
{
    [Header("Star Settings")]
    public int starsPerLayer = 50;
    public int numberOfLayers = 3;
    public Color[] starColors = { Color.white, new Color(0.8f, 0.8f, 1f), new Color(1f, 1f, 0.8f) };
    public float[] layerSpeeds = { 0.5f, 1f, 2f };
    public float[] starSizes = { 0.03f, 0.05f, 0.08f };

    private GameObject[][] stars;
    private float[][] speeds;

    private void Start()
    {
        GenerateStarfield();
    }

    private void GenerateStarfield()
    {
        stars = new GameObject[numberOfLayers][];
        speeds = new float[numberOfLayers][];

        for (int layer = 0; layer < numberOfLayers; layer++)
        {
            stars[layer] = new GameObject[starsPerLayer];
            speeds[layer] = new float[starsPerLayer];
            float baseSpeed = layer < layerSpeeds.Length ? layerSpeeds[layer] : 1f;

            for (int i = 0; i < starsPerLayer; i++)
            {
                GameObject star = new GameObject($"Star_L{layer}_{i}");
                star.transform.SetParent(transform);

                SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
                sr.sprite = CreateStarSprite();

                float size = layer < starSizes.Length ? starSizes[layer] : 0.05f;
                size *= Random.Range(0.5f, 1.5f);
                star.transform.localScale = new Vector3(size, size, 1f);

                Color starColor = layer < starColors.Length ? starColors[layer] : Color.white;
                float brightness = Random.Range(0.5f, 1f);
                sr.color = new Color(starColor.r * brightness, starColor.g * brightness, starColor.b * brightness, brightness);

                sr.sortingLayerName = "Background";
                sr.sortingOrder = layer;

                // Random position within screen bounds
                float x = Random.Range(-10f, 10f);
                float y = Random.Range(-6f, 6f);
                star.transform.position = new Vector3(x, y, 10f - layer);

                stars[layer][i] = star;
                speeds[layer][i] = baseSpeed * Random.Range(0.8f, 1.2f);
            }
        }
    }

    private Sprite CreateStarSprite()
    {
        Texture2D tex = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < 16; i++)
            pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }

    private void Update()
    {
        for (int layer = 0; layer < numberOfLayers; layer++)
        {
            for (int i = 0; i < starsPerLayer; i++)
            {
                if (stars[layer][i] == null) continue;

                Transform t = stars[layer][i].transform;
                t.Translate(Vector3.down * speeds[layer][i] * Time.deltaTime, Space.World);

                // Wrap around when off screen
                if (t.position.y < -6f)
                {
                    t.position = new Vector3(
                        Random.Range(-10f, 10f),
                        6f + Random.Range(0f, 1f),
                        t.position.z
                    );
                }
            }
        }
    }
}
