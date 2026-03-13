using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Scrolling Settings")]
    public float scrollSpeed = 2f;
    public float resetPositionY = 10f;
    public float startPositionY = -10f;

    [Header("Star Field Settings")]
    public int numberOfStars = 100;
    public float starFieldWidth = 18f;
    public float starFieldHeight = 12f;

    private Transform[] stars;
    private SpriteRenderer[] starRenderers;
    private float[] starSpeeds;

    void Start()
    {
        CreateStarField();
    }

    void CreateStarField()
    {
        stars = new Transform[numberOfStars];
        starRenderers = new SpriteRenderer[numberOfStars];
        starSpeeds = new float[numberOfStars];

        for (int i = 0; i < numberOfStars; i++)
        {
            GameObject star = new GameObject("Star_" + i);
            star.transform.SetParent(transform);

            float x = Random.Range(-starFieldWidth / 2f, starFieldWidth / 2f);
            float y = Random.Range(-starFieldHeight / 2f, starFieldHeight / 2f);
            star.transform.position = new Vector3(x, y, 1f);

            SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
            sr.sprite = CreateStarSprite();

            float depth = Random.Range(0.3f, 1f);
            float size = depth * 0.15f;
            star.transform.localScale = new Vector3(size, size, 1f);

            float brightness = depth;
            sr.color = new Color(brightness, brightness, brightness, brightness);

            sr.sortingOrder = -10;

            stars[i] = star.transform;
            starRenderers[i] = sr;
            starSpeeds[i] = scrollSpeed * depth;
        }
    }

    Sprite CreateStarSprite()
    {
        Texture2D texture = new Texture2D(4, 4);
        Color[] colors = new Color[16];
        for (int i = 0; i < 16; i++)
        {
            colors[i] = Color.white;
        }
        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
    }

    void Update()
    {
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                stars[i].position += Vector3.down * starSpeeds[i] * Time.deltaTime;

                if (stars[i].position.y < -starFieldHeight / 2f)
                {
                    float x = Random.Range(-starFieldWidth / 2f, starFieldWidth / 2f);
                    stars[i].position = new Vector3(x, starFieldHeight / 2f, 1f);
                }
            }
        }
    }

    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;
        for (int i = 0; i < starSpeeds.Length; i++)
        {
            float depth = starRenderers[i].color.r;
            starSpeeds[i] = scrollSpeed * depth;
        }
    }
}
