using UnityEngine;

/// <summary>
/// Parallax scrolling background controller.
/// Creates a looping starfield/space background effect with multiple layers.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Layer Settings")]
    [SerializeField] private int layerCount = 3;
    [SerializeField] private float[] layerSpeeds = { 0.5f, 1f, 2f };
    [SerializeField] private Color[] layerColors;

    [Header("Star Settings")]
    [SerializeField] private int starsPerLayer = 40;
    [SerializeField] private float fieldWidth = 20f;
    [SerializeField] private float fieldHeight = 14f;

    private Transform[][] starLayers;

    private void Start()
    {
        if (layerColors == null || layerColors.Length == 0)
        {
            layerColors = new Color[]
            {
                new Color(0.3f, 0.3f, 0.4f),  // Dim distant stars
                new Color(0.6f, 0.6f, 0.8f),  // Medium stars
                new Color(1f, 1f, 1f)           // Bright close stars
            };
        }

        CreateStarfield();
    }

    private void CreateStarfield()
    {
        starLayers = new Transform[layerCount][];

        for (int layer = 0; layer < layerCount; layer++)
        {
            GameObject layerObj = new GameObject($"StarLayer_{layer}");
            layerObj.transform.parent = transform;

            starLayers[layer] = new Transform[starsPerLayer];

            float starSize = 0.02f + layer * 0.02f; // Bigger stars for closer layers
            Color starColor = layer < layerColors.Length ? layerColors[layer] : Color.white;

            for (int i = 0; i < starsPerLayer; i++)
            {
                GameObject star = new GameObject($"Star_{layer}_{i}");
                star.transform.parent = layerObj.transform;

                SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteGenerator.CreateCircleSprite(4, Color.white);
                sr.color = starColor;
                sr.sortingOrder = -10 + layer;

                float x = Random.Range(-fieldWidth / 2f, fieldWidth / 2f);
                float y = Random.Range(-fieldHeight / 2f, fieldHeight / 2f);
                star.transform.position = new Vector3(x, y, 10f - layer);
                star.transform.localScale = Vector3.one * (starSize + Random.Range(-0.01f, 0.01f));

                starLayers[layer][i] = star.transform;
            }
        }
    }

    private void Update()
    {
        if (starLayers == null) return;

        for (int layer = 0; layer < layerCount; layer++)
        {
            float speed = layer < layerSpeeds.Length ? layerSpeeds[layer] : 1f;

            for (int i = 0; i < starLayers[layer].Length; i++)
            {
                Transform star = starLayers[layer][i];
                if (star == null) continue;

                star.position += Vector3.down * speed * Time.deltaTime;

                // Wrap around
                if (star.position.y < -fieldHeight / 2f)
                {
                    float x = Random.Range(-fieldWidth / 2f, fieldWidth / 2f);
                    star.position = new Vector3(x, fieldHeight / 2f, star.position.z);
                }
            }
        }
    }
}
