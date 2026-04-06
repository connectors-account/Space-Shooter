using UnityEngine;

/// <summary>
/// Creates and manages a parallax scrolling starfield background.
/// Uses multiple layers of star objects moving at different speeds for depth effect.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Starfield Settings")]
    public int starsPerLayer = 50;
    public int numberOfLayers = 3;
    public float baseSpeed = 1f;
    public float speedMultiplierPerLayer = 1.5f;

    [Header("Star Appearance")]
    public float minStarSize = 0.02f;
    public float maxStarSize = 0.08f;

    private StarLayer[] layers;

    private struct Star
    {
        public GameObject obj;
        public float speed;
    }

    private class StarLayer
    {
        public Star[] stars;
        public float layerSpeed;
    }

    private void Start()
    {
        CreateStarfield();
    }

    private void CreateStarfield()
    {
        layers = new StarLayer[numberOfLayers];

        for (int layer = 0; layer < numberOfLayers; layer++)
        {
            StarLayer starLayer = new StarLayer();
            starLayer.layerSpeed = baseSpeed * Mathf.Pow(speedMultiplierPerLayer, layer);
            starLayer.stars = new Star[starsPerLayer];

            // Deeper layers are dimmer and smaller
            float brightness = 0.3f + (float)layer / numberOfLayers * 0.7f;
            float starSize = Mathf.Lerp(minStarSize, maxStarSize, (float)layer / numberOfLayers);

            Color starColor = new Color(brightness, brightness, brightness * 1.1f, 1f);
            Sprite starSprite = SpriteGenerator.CreateCircleSprite(4, starColor);

            for (int i = 0; i < starsPerLayer; i++)
            {
                GameObject starObj = new GameObject($"Star_L{layer}_{i}");
                starObj.transform.parent = transform;

                SpriteRenderer sr = starObj.AddComponent<SpriteRenderer>();
                sr.sprite = starSprite;
                sr.sortingOrder = -10 + layer;

                // Random position across the visible area
                float x = Random.Range(-10f, 10f);
                float y = Random.Range(-6f, 6f);
                starObj.transform.position = new Vector3(x, y, 10f - layer);
                starObj.transform.localScale = Vector3.one * starSize * (1f + Random.Range(-0.3f, 0.3f));

                Star star = new Star();
                star.obj = starObj;
                star.speed = starLayer.layerSpeed * (1f + Random.Range(-0.2f, 0.2f));
                starLayer.stars[i] = star;
            }

            layers[layer] = starLayer;
        }
    }

    private void Update()
    {
        if (layers == null) return;

        float topBound = 6.5f;
        float bottomBound = -6.5f;

        foreach (StarLayer layer in layers)
        {
            foreach (Star star in layer.stars)
            {
                if (star.obj == null) continue;

                // Move stars downward
                star.obj.transform.Translate(Vector3.down * star.speed * Time.deltaTime);

                // Wrap around when going off-screen
                if (star.obj.transform.position.y < bottomBound)
                {
                    Vector3 pos = star.obj.transform.position;
                    pos.y = topBound;
                    pos.x = Random.Range(-10f, 10f);
                    star.obj.transform.position = pos;
                }
            }
        }
    }
}
