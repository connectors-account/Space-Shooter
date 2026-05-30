using UnityEngine;

/// <summary>
/// StarfieldBackground – Creates a scrolling star-field effect using particles.
/// Attach to the Main Camera or an empty GameObject in any scene.
/// Makes the background feel like space without needing textures.
/// </summary>
public class StarfieldBackground : MonoBehaviour
{
    [Header("Star Settings")]
    [Tooltip("Number of stars")]
    public int starCount = 100;

    [Tooltip("Scroll speed (how fast stars drift downward)")]
    public float scrollSpeed = 1.5f;

    // Internal
    private Transform[] stars;
    private float screenTop, screenBottom;
    private float screenLeft, screenRight;

    void Start()
    {
        Camera cam = Camera.main;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        screenTop = halfHeight + 0.5f;
        screenBottom = -halfHeight - 0.5f;
        screenLeft = -halfWidth;
        screenRight = halfWidth;

        stars = new Transform[starCount];

        for (int i = 0; i < starCount; i++)
        {
            GameObject star = new GameObject("Star_" + i);
            star.transform.SetParent(transform);

            SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
            sr.sprite = CreateDotSprite();

            // Random size and brightness
            float size = Random.Range(0.02f, 0.08f);
            float brightness = Random.Range(0.3f, 1f);
            star.transform.localScale = Vector3.one * size;
            sr.color = new Color(brightness, brightness, brightness, brightness);
            sr.sortingOrder = -10; // behind everything

            // Random starting position
            float x = Random.Range(screenLeft, screenRight);
            float y = Random.Range(screenBottom, screenTop);
            star.transform.position = new Vector3(x, y, 10f);

            stars[i] = star.transform;
        }
    }

    void Update()
    {
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null) continue;

            Vector3 pos = stars[i].position;
            pos.y -= scrollSpeed * Time.unscaledDeltaTime *
                     (stars[i].localScale.x / 0.05f); // parallax: bigger = faster

            if (pos.y < screenBottom)
            {
                pos.y = screenTop;
                pos.x = Random.Range(screenLeft, screenRight);
            }
            stars[i].position = pos;
        }
    }

    /// <summary>Create a tiny white dot sprite at runtime.</summary>
    static Sprite CreateDotSprite()
    {
        Texture2D tex = new Texture2D(4, 4);
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                tex.SetPixel(x, y, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
    }
}
