using UnityEngine;

/// <summary>
/// Creates a scrolling starfield background using particles.
/// Attach to a dedicated "Background" empty GameObject.
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header("Star Settings")]
    public int starCount = 80;
    public float scrollSpeed = 1.5f;
    public float starMinSize = 0.02f;
    public float starMaxSize = 0.08f;

    private Transform[] stars;
    private float[] starSpeeds;
    private SpriteRenderer[] starRenderers;
    private float screenTop, screenBottom, screenLeft, screenRight;

    void Start()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        screenTop = cam.ViewportToWorldPoint(Vector3.up).y + 1f;
        screenBottom = cam.ViewportToWorldPoint(Vector3.zero).y - 1f;
        screenLeft = cam.ViewportToWorldPoint(Vector3.zero).x - 1f;
        screenRight = cam.ViewportToWorldPoint(Vector3.right).x + 1f;

        CreateStars();
    }

    void CreateStars()
    {
        stars = new Transform[starCount];
        starSpeeds = new float[starCount];
        starRenderers = new SpriteRenderer[starCount];

        for (int i = 0; i < starCount; i++)
        {
            GameObject star = new GameObject("Star_" + i);
            star.transform.parent = transform;

            float x = Random.Range(screenLeft, screenRight);
            float y = Random.Range(screenBottom, screenTop);
            star.transform.position = new Vector3(x, y, 1f); // Behind everything

            SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Square");
            sr.sortingOrder = -10;

            float size = Random.Range(starMinSize, starMaxSize);
            star.transform.localScale = Vector3.one * size;

            // Dimmer stars move slower (parallax)
            float brightness = Random.Range(0.3f, 1f);
            sr.color = new Color(brightness, brightness, brightness * 1.1f, brightness);

            stars[i] = star.transform;
            starSpeeds[i] = scrollSpeed * (0.3f + brightness * 0.7f);
            starRenderers[i] = sr;
        }
    }

    void Update()
    {
        if (stars == null) return;

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null) continue;

            stars[i].position += Vector3.down * starSpeeds[i] * Time.deltaTime;

            // Wrap around
            if (stars[i].position.y < screenBottom)
            {
                float x = Random.Range(screenLeft, screenRight);
                stars[i].position = new Vector3(x, screenTop, 1f);
            }
        }
    }
}
