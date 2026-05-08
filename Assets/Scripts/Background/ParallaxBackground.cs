using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Creates a parallax star field background with multiple layers moving at different speeds.
/// Stars are recycled as they move off-screen for infinite scrolling.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int starsPerLayer = 40;
    [SerializeField] private int layerCount = 3;
    [SerializeField] private float baseScrollSpeed = 1f;

    private List<StarLayer> layers = new List<StarLayer>();

    private class StarLayer
    {
        public float speed;
        public Transform[] stars;
        public float starSize;
    }

    private void Start()
    {
        CreateStarField();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing &&
            GameManager.Instance.CurrentState != GameState.Paused) return;

        foreach (var layer in layers)
        {
            foreach (var star in layer.stars)
            {
                if (star == null) continue;

                star.position += Vector3.down * layer.speed * Time.deltaTime;

                // Recycle star when it goes below screen
                if (star.position.y < -(GameManager.Instance?.gameBoundsY ?? 6f) - 1f)
                {
                    float boundsX = GameManager.Instance?.gameBoundsX ?? 9f;
                    float boundsY = GameManager.Instance?.gameBoundsY ?? 6f;
                    star.position = new Vector3(
                        Random.Range(-boundsX, boundsX),
                        boundsY + Random.Range(0.5f, 2f),
                        star.position.z
                    );
                }
            }
        }
    }

    private void CreateStarField()
    {
        float boundsX = GameManager.Instance?.gameBoundsX ?? 9f;
        float boundsY = GameManager.Instance?.gameBoundsY ?? 6f;

        for (int layer = 0; layer < layerCount; layer++)
        {
            StarLayer sl = new StarLayer();

            // Deeper layers move slower and have smaller/dimmer stars
            float depth = (float)(layer + 1) / layerCount;
            sl.speed = baseScrollSpeed * depth;
            sl.starSize = 0.03f + depth * 0.06f;

            float brightness = 0.3f + depth * 0.7f;
            Color starColor = new Color(brightness, brightness, brightness + 0.1f);

            int starSize = Mathf.Max(2, (int)(4 * depth));
            Sprite starSprite = SpriteFactory.CreateStar(starSize, starColor);

            sl.stars = new Transform[starsPerLayer];

            for (int i = 0; i < starsPerLayer; i++)
            {
                GameObject star = new GameObject($"Star_L{layer}_{i}");
                star.transform.SetParent(transform);
                star.transform.position = new Vector3(
                    Random.Range(-boundsX, boundsX),
                    Random.Range(-boundsY, boundsY),
                    10f + layer // Behind everything
                );
                star.transform.localScale = Vector3.one * sl.starSize;

                SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
                sr.sprite = starSprite;
                sr.sortingOrder = -10 - layer;

                sl.stars[i] = star.transform;
            }

            layers.Add(sl);
        }

        // Add a few colored nebula-like larger dim stars for atmosphere
        AddNebulaAccents(boundsX, boundsY);
    }

    private void AddNebulaAccents(float boundsX, float boundsY)
    {
        Color[] nebulaColors = new Color[]
        {
            new Color(0.2f, 0.1f, 0.4f, 0.15f),
            new Color(0.1f, 0.2f, 0.4f, 0.1f),
            new Color(0.3f, 0.1f, 0.2f, 0.12f),
        };

        for (int i = 0; i < 6; i++)
        {
            GameObject nebula = new GameObject($"Nebula_{i}");
            nebula.transform.SetParent(transform);
            nebula.transform.position = new Vector3(
                Random.Range(-boundsX, boundsX),
                Random.Range(-boundsY, boundsY),
                15f
            );

            SpriteRenderer sr = nebula.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.CreateCircle(32, nebulaColors[i % nebulaColors.Length]);
            sr.sortingOrder = -20;

            float scale = Random.Range(2f, 5f);
            nebula.transform.localScale = Vector3.one * scale;
        }
    }
}
